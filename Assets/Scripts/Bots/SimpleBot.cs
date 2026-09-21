using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.Player;

namespace CrabPrototype.Bots
{
    /// <summary>
    /// IA simple pero caótica: busca objetivo según modo (sombrero, colina, salida),
    /// empuja a cercanos, salta obstáculos. Suficiente para prototipo divertido.
    /// </summary>
    [RequireComponent(typeof(CrabPlayer))]
    public class SimpleBot : MonoBehaviour
    {
        CrabPlayer me;
        float repath;
        Vector3 target;
        float pushCheck;

        void Awake() { me = GetComponent<CrabPlayer>(); }

        void Update()
        {
            if (me.data == null || !me.data.alive) return;
            repath -= Time.deltaTime;
            if (repath <= 0) { repath = 0.5f + Random.value; target = PickTarget(); }

            Vector3 dir = target - transform.position; dir.y = 0;
            bool jump = false;
            // Salta si hay muro delante
            if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, 1.5f))
                jump = true;
            // Saltos aleatorios para caos
            if (Random.value < 0.01f) jump = true;

            me.MoveExternal(dir.normalized * (dir.magnitude > 1f ? 1f : 0.4f), jump);

            // Empuja a cercanos
            pushCheck -= Time.deltaTime;
            if (pushCheck <= 0)
            {
                pushCheck = 0.4f + Random.value * 0.8f;
                Collider[] hits = Physics.OverlapSphere(transform.position, 2.2f);
                foreach (var h in hits)
                {
                    var o = h.GetComponentInParent<CrabPlayer>();
                    if (o != null && o != me && Random.value < 0.7f) { me.TryPush(); break; }
                }
                if (me.hasStick && Random.value < 0.5f) me.TryStickHit();
            }
        }

        Vector3 PickTarget()
        {
            var gm = GameManager.Instance;
            if (gm == null) return transform.position + Random.insideUnitSphere * 10;

            switch (gm.currentMode)
            {
                case GameModeType.HatKing:
                    // Ir al del sombrero
                    foreach (var p in gm.players)
                        if (p.alive && p.hasHat && p.controller != me)
                            return p.controller.transform.position;
                    return transform.position + Random.insideUnitSphere * 12;

                case GameModeType.KingOfHill:
                    var hill = GameObject.Find("HillZone");
                    if (hill) return hill.transform.position;
                    break;

                case GameModeType.GlassFloor:
                case GameModeType.Race:
                    // Ir hacia +Z (meta)
                    return transform.position + new Vector3(Random.Range(-4, 4), 0, 12);

                case GameModeType.FloorIsLava:
                    // Buscar plataforma alta cercana
                    var plats = GameObject.FindGameObjectsWithTag("Platform");
                    float best = float.MaxValue; Vector3 bp = transform.position;
                    foreach (var pl in plats)
                    {
                        float d = Vector3.Distance(transform.position, pl.transform.position);
                        if (d < best && pl.transform.position.y > 1f) { best = d; bp = pl.transform.position; }
                    }
                    return bp;

                case GameModeType.BombTag:
                    if (me.data.hasBomb)
                    {
                        // Huir hacia el más cercano para pasarla
                        float bd = float.MaxValue; Vector3 bt = transform.position;
                        foreach (var p in gm.players)
                        {
                            if (p.controller == me || !p.alive) continue;
                            float d = Vector3.Distance(transform.position, p.controller.transform.position);
                            if (d < bd) { bd = d; bt = p.controller.transform.position; }
                        }
                        return bt;
                    }
                    else
                    {
                        // Huir del de la bomba
                        foreach (var p in gm.players)
                            if (p.alive && p.hasBomb && p.controller != me)
                                return transform.position + (transform.position - p.controller.transform.position).normalized * 10;
                    }
                    break;
            }
            return transform.position + new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));
        }
    }
}
