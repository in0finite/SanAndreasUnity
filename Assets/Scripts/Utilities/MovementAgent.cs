using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SanAndreasUnity.Utilities
{
    public class MovementAgent : MonoBehaviour
    {
        public NavMeshAgent NavMeshAgent { get; private set; }

        private double m_lastTimeWhenSearchedForPath = 0f;

        public Vector3? Destination { get; set; } = null;
        private Vector3? m_lastAssignedDestination = null;
        private Vector3? m_lastPositionWhenAssignedDestination = null;

        private double m_lastTimeWhenWarped = 0f;
        private double m_timeWhenSampledOffNavMesh = 0f;

        public float warpSampleDistance = 4.5f;

        public Vector3 DesiredDirection
        {
            get
            {
                if (!m_isMovingOffNavMesh)
                    return this.NavMeshAgent.desiredVelocity.NormalizedOrZero();

                // agent is not on nav mesh

                if (!this.Destination.HasValue && !m_sampledPosOffNavMesh.HasValue)
                    return Vector3.zero;

                Vector3 myPosition = this.NavMeshAgent.transform.position;
                float stoppingDistance = this.StoppingDistance;

                // if we are in range of destination, don't move
                if (this.Destination.HasValue && Vector3.Distance(this.Destination.Value, myPosition) <= stoppingDistance)
                    return Vector3.zero;

                Vector3 effectiveDestination = m_sampledPosOffNavMesh ?? this.Destination.Value;

                Vector3 diff = effectiveDestination - myPosition;
                return diff.normalized;
            }
        }

        public Vector3 DesiredDirectionXZ
        {
            get
            {
                Vector3 desiredDir = this.DesiredDirection;
                if (desiredDir.y == 0f)
                    return desiredDir;
                return desiredDir.WithXAndZ().NormalizedOrZero();
            }
        }

        public Vector3? CalculatedDestination { get; private set; } = null;

        private bool m_isMovingOffNavMesh = false;
        private Vector3? m_sampledPosOffNavMesh = null;
        
        public float StoppingDistance
        {
            get => this.NavMeshAgent.stoppingDistance;
            set => this.NavMeshAgent.stoppingDistance = value;
        }



        void Awake()
        {
            this.NavMeshAgent = this.GetComponentOrThrow<NavMeshAgent>();

            this.NavMeshAgent.updatePosition = false;
            this.NavMeshAgent.updateRotation = false;
            this.NavMeshAgent.updateUpAxis = false;
        }

        public void RunUpdate()
        {
            this.Update();
        }

        void Update()
        {

            NavMeshAgent agent = this.NavMeshAgent;

            if (!agent.enabled)
            {
                this.ResetParams();
                return;
            }

            double currentTime = Time.timeAsDouble;
            Vector3 myPosition = agent.transform.position;

            agent.nextPosition = myPosition;

            Vector3 retreivedNextPosition = agent.nextPosition;

            if (currentTime - m_lastTimeWhenWarped > 1f
                && (retreivedNextPosition.WithXAndZ() != myPosition.WithXAndZ() || !agent.isOnNavMesh))
            {
                m_lastTimeWhenWarped = currentTime;

                bool bWarp = false;
                bool bSetDestination = false;
                
                // here we sample position to prevent Unity to spam with warning messages saying that agent is
                // not close to nav mesh
                if (NavMesh.SamplePosition(myPosition, out var hit, this.warpSampleDistance, agent.areaMask)
                    && agent.Warp(myPosition))
                {
                    bWarp = true;
                    if (this.Destination.HasValue && agent.isOnNavMesh)
                    {
                        this.SetDestination();
                        bSetDestination = true;
                    }
                }

                //Debug.Log($"warped agent {agent.name} - bWarp {bWarp}, isOnNavMesh {agent.isOnNavMesh}, pos diff {retreivedNextPosition - myPosition}, bSetDestination {bSetDestination}", this);
            }

            // no need to set velocity, it's automatically set by Agent
            //this.NavMeshAgent.velocity = this.Velocity;

            // update calculated destination
            this.CalculatedDestination = agent.hasPath ? agent.destination : (Vector3?)null;

            // if agent is off nav mesh, try to get it back
            if (!agent.isOnNavMesh)
            {
                m_isMovingOffNavMesh = true; // immediately start moving when agent goes off the nav mesh
                if (currentTime - m_timeWhenSampledOffNavMesh > 2.5f)
                {
                    // try to sample position on nav mesh where agent could go

                    m_timeWhenSampledOffNavMesh = currentTime;
                    m_sampledPosOffNavMesh = null;

                    if (NavMesh.SamplePosition(myPosition, out var hit, 150f, agent.areaMask))
                    {
                        m_sampledPosOffNavMesh = hit.position;
                    }

                    //Debug.Log($"Tried to sample position off nav mesh - agent {agent.name}, sampled pos {m_sampledPosOffNavMesh}, distance {hit.distance}", this);
                }
            }
            else
            {
                m_isMovingOffNavMesh = false;
                m_sampledPosOffNavMesh = null;
            }

            if (!this.Destination.HasValue)
            {
                this.ResetParams();
                return;
            }

            if (currentTime - m_lastTimeWhenSearchedForPath < 0.4f)
                return;

            if (agent.pathPending)
                return;

            if (!agent.isOnNavMesh)
                return;

            if (!m_lastAssignedDestination.HasValue)
            {
                this.SetDestination();
                return;
            }

            // check if target position changed by some delta value (this value should depend on distance to target
            // - if target is too far away, value should be higher)

            Vector3 diffToTarget = this.Destination.Value - myPosition;
            float distanceToTarget = diffToTarget.magnitude;
            Vector3 deltaPos = this.Destination.Value - m_lastAssignedDestination.Value;
            float deltaPosLength = deltaPos.magnitude;

            // we require 10% change, with 1.5 as min
            float requiredPosChange = Mathf.Max(distanceToTarget * 0.1f, 1.5f);

            if (deltaPosLength > requiredPosChange)
            {
                this.SetDestination();
                return;
            }

            // check if angle to target changed by some delta value (eg. 25 degrees)
            // - this will make the ped turn fast in response to target changing movement direction

            Vector3 lastDiffToTarget = m_lastAssignedDestination.Value - m_lastPositionWhenAssignedDestination.Value;
            float angleDelta = Vector3.Angle(this.Destination.Value - m_lastPositionWhenAssignedDestination.Value, lastDiffToTarget);
            if (angleDelta > 25f)
            {
                this.SetDestination();
                return;
            }

            // regularly update path on some higher interval (eg. 5s)
            // - this interval could also depend on distance to target

            // from 5 to 12, with sqrt function, 150 as max distance
            float regularUpdateInterval = 5 + 7 * Mathf.Clamp01(Mathf.Sqrt(Mathf.Min(distanceToTarget, 150f) / 150f));

            if (currentTime - m_lastTimeWhenSearchedForPath > regularUpdateInterval
                && this.Destination.Value != m_lastAssignedDestination.Value)
            {
                this.SetDestination();
                return;
            }

            // handle cases when destination changes by significant amount, but it's not recognized
            // by "delta position" method above
            // - this happens when position delta is too small, but Agent should still do re-path
            // (for example, he needs to touch the destination object)

            float deltaInPosition = (this.Destination.Value - m_lastAssignedDestination.Value).magnitude;
            float currentDistance = (m_lastAssignedDestination.Value - myPosition).magnitude;
            if (deltaInPosition > currentDistance)
            {
                Debug.Log($"delta pos higher than current distance - agent {agent.name}, delta {deltaInPosition}, current distance {currentDistance}", this);
                this.SetDestination();
                return;
            }

            // handle case caused by bug in NavMesh system - it can not calculate paths that are too
            // long (or have too many corners). The path returned has status Partial, and it's final
            // corner is not even close to destination. Not only that it doesn't return full path, but it actually
            // returns path which contain closest point to destination, which can be totally wrong path.

            // be careful not to mix this case with regular partial paths that happen because destination is really
            // not reachable. This can actually happen quite often, if destination is, for example, on nearby roof.

            float stoppingDistance = this.StoppingDistance;
            float distanceToCalculatedDestination = this.CalculatedDestination.HasValue ? Vector3.Distance(this.CalculatedDestination.Value, myPosition) : float.PositiveInfinity;
            float originalDistanceToCalculatedDestination = this.CalculatedDestination.HasValue ? Vector3.Distance(m_lastPositionWhenAssignedDestination.Value, this.CalculatedDestination.Value) : float.PositiveInfinity;

            if (this.CalculatedDestination.HasValue
                && currentTime - m_lastTimeWhenSearchedForPath > 3f // seems like it's not needed, but just in case
                && originalDistanceToCalculatedDestination > 50f // this will make a difference between regular partial paths
                && originalDistanceToCalculatedDestination > stoppingDistance + 3f // also need to handle case when stopping distance is too large
                && (distanceToCalculatedDestination < 4f || distanceToCalculatedDestination <= stoppingDistance) // already stopped or close enough
                && agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Debug.Log($"re-path due to bug in NavMesh system - agent {agent.name}, distanceToCalculatedDestination {distanceToCalculatedDestination}, originalDistanceToCalculatedDestination {originalDistanceToCalculatedDestination}", this);
                this.SetDestination();
                return;
            }

            // 2nd solution for problem above

            float distanceTraveled = (myPosition - m_lastPositionWhenAssignedDestination.Value).magnitude;

            if (currentTime - m_lastTimeWhenSearchedForPath > 3f
                && distanceTraveled > 200f
                && agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Debug.Log($"re-path due to bug in NavMesh system #2 - agent {agent.name}, distanceTraveled {distanceTraveled}", this);
                this.SetDestination();
                return;
            }

        }

        void SetDestination()
        {
            NavMeshAgent navMeshAgent = this.NavMeshAgent;

            m_lastTimeWhenSearchedForPath = Time.timeAsDouble;
            m_lastAssignedDestination = this.Destination.Value;
            m_lastPositionWhenAssignedDestination = navMeshAgent.transform.position;
            
            // here we need to sample position on navmesh first, because otherwise agent will fail
            // to calculate path if target position is not on navmesh, and as a result he will be stopped

            // there is a performance problem: if target position is on isolated part of navmesh,
            // path calculation will take too long because the algorithm tries to go through all
            // surrounding nodes, and in the meantime agent stays in place

            // that's why we manually calculate path and assign it to agent - in this case, there is no waiting
            // for path to be calculated asyncly, and agent starts moving immediately. The potential problem
            // is that CalculatePath() can take 1-2 ms.

            // TODO: performance optimization: this can be done "asyncly": register pathfinding request, and process
            // requests from all agents in Update() function of some Manager script, with some time limit (eg. 1 ms)

            if (NavMesh.SamplePosition(this.Destination.Value, out var hit, 100f, navMeshAgent.areaMask))
            {
                // TODO: re-use NavMeshPath object
                var navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(navMeshAgent.nextPosition, hit.position, navMeshAgent.areaMask, navMeshPath);
                navMeshAgent.path = navMeshPath;

                this.CalculatedDestination = navMeshAgent.hasPath ? navMeshAgent.destination : (Vector3?)null;
            }
            else
            {
                // if position can not be sampled, we stop the agent
                navMeshAgent.ResetPath();
                this.CalculatedDestination = null;
            }
        }

        private void ResetParams()
        {
            m_lastAssignedDestination = null;
            m_lastPositionWhenAssignedDestination = null;
            this.CalculatedDestination = null;

            if (this.NavMeshAgent.hasPath)
                this.NavMeshAgent.ResetPath();
        }

        private void OnDrawGizmosSelected()
        {
            if (null == this.NavMeshAgent)
                return;

            if (!this.NavMeshAgent.hasPath)
                return;

            Gizmos.color = Color.Lerp(Color.red, Color.black, 0.5f);

            Vector3[] corners = this.NavMeshAgent.path.corners;
            for (int i = 1; i < corners.Length; i++)
            {
                Gizmos.DrawWireSphere(corners[i], 0.75f);
                Gizmos.DrawLine(corners[i - 1], corners[i]);
            }
        }
    }
}
