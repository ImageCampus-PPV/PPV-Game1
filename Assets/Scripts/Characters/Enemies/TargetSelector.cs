using UnityEngine;

public static class TargetSelector
{
    public static Transform GetBestTarget(Vector3 origin, float range, LayerMask targetLayer)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, targetLayer);

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable damageable = hits[i].GetComponent<IDamageable>();

            if (damageable != null)
            {
                float dist = Vector2.Distance(origin, hits[i].transform.position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hits[i].transform;
                }
            }
        }

        if (closest == null) return null;

        if (Random.value > 0.5f)
        {
            return hits[Random.Range(0, hits.Length)].transform;
        }

        return closest;
    }
}