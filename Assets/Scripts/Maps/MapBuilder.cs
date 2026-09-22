using UnityEngine;
using CrabPrototype.Core;

namespace CrabPrototype.Maps
{
    /// <summary>
    /// Construye arenas low-poly por código (sin assets).
    /// Cada modo tiene su mapa: arena, colina, puente de cristal, pista, lava.
    /// Todo con primitivas + colores planos estilo Crab Game.
    /// </summary>
    public class MapBuilder : MonoBehaviour
    {
        GameObject mapRoot;
        Material flatMat(Color c)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = c;
            m.SetFloat("_Glossiness", 0f);
            return m;
        }

        public void BuildForMode(GameModeType mode, GameManager gm)
        {
            if (mapRoot != null) Destroy(mapRoot);
            mapRoot = new GameObject($"Map_{mode}");
            // Luz + cielo
            SetupLightAndSky();
            switch (mode)
            {
                case GameModeType.HatKing:
                case GameModeType.BombTag:
                case GameModeType.StickFight:
                    BuildArena(18f, new Color(0.45f, 0.75f, 0.45f));
                    break;
                case GameModeType.KingOfHill:
                    BuildArena(24f, new Color(0.55f, 0.7f, 0.5f));
                    BuildHill();
                    break;
                case GameModeType.GlassFloor:
                    BuildGlassBridge();
                    break;
                case GameModeType.Race:
                    BuildRaceTrack();
                    break;
                case GameModeType.FloorIsLava:
                    BuildLavaArena();
                    break;
            }
        }

        void SetupLightAndSky()
        {
            if (FindObjectOfType<Light>() == null)
            {
                var l = new GameObject("Sun").AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.2f;
                l.transform.rotation = Quaternion.Euler(50, 30, 0);
            }
            Camera cam = Camera.main;
            if (cam != null) cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        }

        void Ground(Vector3 pos, Vector3 size, Color c, string name = "Ground")
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.SetParent(mapRoot.transform);
            g.transform.position = pos;
            g.transform.localScale = size;
            g.GetComponent<Renderer>().material = flatMat(c);
        }

        void BuildArena(float radius, Color groundColor)
        {
            // Suelo circular aproximado con cilindro
            var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            g.name = "Ground";
            g.transform.SetParent(mapRoot.transform);
            g.transform.position = Vector3.zero;
            g.transform.localScale = new Vector3(radius, 1, radius);
            g.GetComponent<Renderer>().material = flatMat(groundColor);

            // Bordes bajos (para que puedas caer si te empujan fuerte -> caos)
            // Sin paredes altas a propósito: en StickFight/BombTag caer = eliminado.
            // Pareditas decorativas
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2;
                var cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cone.transform.SetParent(mapRoot.transform);
                cone.transform.position = new Vector3(Mathf.Cos(a) * (radius * 0.55f), 1.5f, Mathf.Sin(a) * (radius * 0.55f));
                cone.transform.localScale = new Vector3(1.5f, 3f, 1.5f);
                cone.GetComponent<Renderer>().material = flatMat(new Color(0.3f, 0.55f, 0.35f));
            }
            // Obstáculos centrales
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.SetParent(mapRoot.transform);
            box.transform.position = new Vector3(0, 1, 0);
            box.transform.localScale = new Vector3(3, 2, 3);
            box.GetComponent<Renderer>().material = flatMat(new Color(0.9f, 0.8f, 0.5f));
        }

        void BuildHill()
        {
            var hill = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hill.name = "HillZone";
            hill.transform.SetParent(mapRoot.transform);
            hill.transform.position = new Vector3(0, 0.6f, 0);
            hill.transform.localScale = new Vector3(8, 1.2f, 8);
            var mat = flatMat(new Color(1f, 0.85f, 0.3f));
            // Transparente brillante para ver la zona
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.color = new Color(1f, 0.85f, 0.3f, 0.5f);
            hill.GetComponent<Renderer>().material = mat;
            Destroy(hill.GetComponent<Collider>());
            // Base sólida debajo
            var bas = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bas.transform.SetParent(mapRoot.transform);
            bas.transform.position = new Vector3(0, 0.1f, 0);
            bas.transform.localScale = new Vector3(8, 0.4f, 8);
            bas.GetComponent<Renderer>().material = flatMat(new Color(0.95f, 0.75f, 0.25f));
        }

        void BuildGlassBridge()
        {
            // Plataforma inicio / fin
            Ground(new Vector3(0, 0, -10), new Vector3(10, 1, 6), new Color(0.4f, 0.6f, 0.9f), "Start");
            Ground(new Vector3(0, 0, 22), new Vector3(10, 1, 6), new Color(0.4f, 0.9f, 0.5f), "Finish");
            AddFinishTrigger(new Vector3(0, 1.5f, 22), new Vector3(10, 3, 4), GameModeType.GlassFloor);

            // 8 pares de cristales
            for (int row = 0; row < 8; row++)
            {
                bool leftIsReal = Random.value > 0.5f;
                for (int side = -1; side <= 1; side += 2)
                {
                    bool real = (side < 0) ? leftIsReal : !leftIsReal;
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.transform.SetParent(mapRoot.transform);
                    tile.transform.position = new Vector3(side * 1.5f, 0, -5 + row * 3f);
                    tile.transform.localScale = new Vector3(2.4f, 0.4f, 2.4f);
                    var gp = tile.AddComponent<GlassTile>();
                    gp.isReal = real;
                    var r = tile.GetComponent<Renderer>();
                    r.material = flatMat(new Color(0.7f, 0.95f, 1f, 0.85f));
                }
            }
            // Vacío debajo (caer = muerte por altura)
        }

        void BuildRaceTrack()
        {
            // Pista recta larga con obstáculos
            Ground(new Vector3(0, 0, 20), new Vector3(10, 1, 80), new Color(0.85f, 0.6f, 0.4f), "Track");
            // Obstáculos móviles simples (rotan)
            for (int i = 0; i < 5; i++)
            {
                var spin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spin.transform.SetParent(mapRoot.transform);
                spin.transform.position = new Vector3(0, 1.2f, -5 + i * 12f);
                spin.transform.localScale = new Vector3(7, 0.5f, 0.8f);
                spin.GetComponent<Renderer>().material = flatMat(Color.red);
                var sp = spin.AddComponent<SpinnerObstacle>();
                sp.speed = 60 + i * 20;
            }
            // Muros laterales bajos
            Ground(new Vector3(-5.5f, 1, 20), new Vector3(1, 2, 80), new Color(0.5f, 0.5f, 0.6f), "Wall");
            Ground(new Vector3(5.5f, 1, 20), new Vector3(1, 2, 80), new Color(0.5f, 0.5f, 0.6f), "Wall");
            AddFinishTrigger(new Vector3(0, 2, 55), new Vector3(10, 4, 3), GameModeType.Race);
        }

        void BuildLavaArena()
        {
            // Suelo base (se hundirá bajo lava que sube)
            Ground(Vector3.zero, new Vector3(30, 1, 30), new Color(0.3f, 0.3f, 0.35f), "Base");
            // Lava
            var lava = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lava.name = "Lava";
            lava.transform.SetParent(mapRoot.transform);
            lava.transform.position = new Vector3(0, -4, 0);
            lava.transform.localScale = new Vector3(30, 1, 30);
            var lm = flatMat(new Color(1f, 0.3f, 0.1f));
            lm.EnableKeyword("_EMISSION");
            lm.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * 0.8f);
            lava.GetComponent<Renderer>().material = lm;

            // Plataformas
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2;
                var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                p.tag = "Platform";
                p.transform.SetParent(mapRoot.transform);
                float h = 1.5f + (i % 3) * 1.5f;
                p.transform.position = new Vector3(Mathf.Cos(a) * (4 + i % 3 * 3), h, Mathf.Sin(a) * (4 + i % 3 * 3));
                p.transform.localScale = new Vector3(3, 0.6f, 3);
                p.GetComponent<Renderer>().material = flatMat(new Color(0.4f, 0.55f, 0.6f));
            }
            // Torre central alta
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.tag = "Platform";
            tower.transform.SetParent(mapRoot.transform);
            tower.transform.position = new Vector3(0, 4, 0);
            tower.transform.localScale = new Vector3(4, 1, 4);
            tower.GetComponent<Renderer>().material = flatMat(new Color(0.9f, 0.85f, 0.4f));
        }

        void AddFinishTrigger(Vector3 pos, Vector3 size, GameModeType mode)
        {
            var go = new GameObject("Finish");
            go.transform.SetParent(mapRoot.transform);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            var f = go.AddComponent<FinishTrigger>();
            f.mode = mode;
        }
    }

    /// <summary>Cristal falso se rompe al pisarlo.</summary>
    public class GlassTile : MonoBehaviour
    {
        public bool isReal = true;
        bool broken;
        void OnCollisionEnter(Collision c)
        {
            if (broken || isReal) return;
            var p = c.gameObject.GetComponent<Player.CrabPlayer>();
            if (p == null) return;
            broken = true;
            // Se rompe con delay para dar chance de correr
            Invoke(nameof(Break), 0.25f);
        }
        void Break()
        {
            GetComponent<Renderer>().material.color = Color.clear;
            GetComponent<Collider>().enabled = false;
        }
    }

    public class SpinnerObstacle : MonoBehaviour
    {
        public float speed = 80f;
        void Update() => transform.Rotate(Vector3.up * speed * Time.deltaTime);
        void OnCollisionEnter(Collision c)
        {
            var p = c.gameObject.GetComponent<Player.CrabPlayer>();
            if (p != null)
                p.ApplyKnockback((p.transform.position - transform.position).normalized * 14f + Vector3.up * 4f, null);
        }
    }

    public class FinishTrigger : MonoBehaviour
    {
        public GameModeType mode;
        void OnTriggerEnter(Collider other)
        {
            var p = other.GetComponentInParent<Player.CrabPlayer>();
            if (p == null || p.data == null) return;
            var gm = GameManager.Instance;
            if (gm == null || !gm.roundActive) return;
            if (mode == GameModeType.Race && gm.ActiveMode is GameModes.RaceMode r)
                r.OnPlayerFinished(p.data);
            else if (mode == GameModeType.GlassFloor && gm.ActiveMode is GameModes.GlassFloorMode g)
                g.OnPlayerFinished(p.data);
        }
    }
}
