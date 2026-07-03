using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace GrazingAnimals
{
    public class Agent : MonoBehaviour
    {
        [SerializeField] private float _radius = .5f;

        private readonly List<Agent> _neighbours = new();
        private readonly List<(Vector2, float)> _constraints = new();

        public Vector3 DesiredVelocity { get; set; }
        public Vector3 Velocity { get; set; }
        public Food Target { get; set; }

        public float Radius => _radius;

        internal INeighboursProvider NeighboursProvider { get; set; }
        public float MaxSpeed { get; set; } = 5f;

        public void Think(float deltaTime)
        {
            if (!Target)
                return;
         
            Profiler.BeginSample("Think");
            
            if (IsTargetReached())
            {
                Target.Collect();
            }
            
            SelectDesiredVelocity(deltaTime);
            
            Profiler.EndSample();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            foreach (var neighbour in _neighbours)
            {
                Gizmos.DrawLine(transform.localPosition, neighbour.transform.localPosition);
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.localPosition, DesiredVelocity);
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.localPosition, Velocity);
        }

        public void Move(float deltaTime)
        {
            transform.localPosition += Velocity * deltaTime;
        }

        private void SelectDesiredVelocity(float deltaTime)
        {
            var actorPosition = transform.localPosition.To2D();
            var actorVelocity = Velocity.To2D();

            var distanceToTarget = (Target.transform.localPosition.To2D() - actorPosition).magnitude;
            var timeToTarget = distanceToTarget / MaxSpeed;
            var maxFramesHorizon = 10;
            var timeHorizon = Mathf.Min(timeToTarget, Mathf.Max(.5f, maxFramesHorizon * deltaTime));
            var neighbourDistance = timeHorizon * MaxSpeed;

            NeighboursProvider?.CollectNeighbours(
                this,
                neighbourDistance,
                _neighbours);
            
            _constraints.Clear();
            foreach (var neighbour in _neighbours)
            {
                var neighbourPosition = neighbour.transform.localPosition.To2D();
                var neighbourVelocity = neighbour.Velocity.To2D();
                
                var relativePosition = neighbourPosition - actorPosition;
                var relativeVelocity = actorVelocity - neighbourVelocity;
                var distance = relativePosition.magnitude;
                var combinedRadius = _radius + neighbour._radius;

                if (distance <= combinedRadius)
                {
                    // TODO: Handle collision?
                    
                    distance = combinedRadius;
                }
                
                var normal = relativePosition.normalized;
                var velocityProjection = Vector2.Dot(relativeVelocity, normal);

                var u = velocityProjection - (distance - combinedRadius) / timeHorizon;

                if (u > 0)
                {
                    var correction = u * normal;
                    var halfCorrection = .5f * correction;
                    var constraint = Vector2.Dot(actorVelocity - halfCorrection, normal);
                    
                    _constraints.Add((normal, constraint));
                }
            }

            var desiredVelocity = (Target.transform.localPosition.To2D() - actorPosition).normalized * MaxSpeed;
            DesiredVelocity = desiredVelocity.To3D();
            var velocity = desiredVelocity;

            const int iterations = 10;
            for (var i = 0; i < iterations; i++)
            {
                foreach (var (normal, constraint) in _constraints)
                {
                    var dot = Vector2.Dot(velocity, normal);
                    if (dot > constraint)
                    {
                        velocity += (constraint - dot) * normal;
                    }
                }
            }

            velocity = Vector3.ClampMagnitude(velocity, MaxSpeed);

            Velocity = velocity.To3D();
        }

        private bool IsTargetReached()
        {
            return Vector3.Distance(transform.localPosition, Target.transform.localPosition) <= 1;
        }
    }
}