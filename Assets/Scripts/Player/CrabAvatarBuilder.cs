using UnityEngine;

namespace CrabPrototype.Player
{
    /// <summary>
    /// Construye un avatar low-poly estilo Crab Game por código
    /// (sin assets externos): cuerpo cápsula de color, ojos saltones y pinzas.
    /// </summary>
    public static class CrabAvatarBuilder
    {
        static readonly Color[] CrabColors = new[]
        {
            new Color(1f, 0.35f, 0.3f), new Color(1f, 0.7f, 0.2f),
            new Color(0.35f, 0.8f, 1f), new Color(0.45f, 1f, 0.5f),
            new Color(0.8f, 0.5f, 1f), new Color(1f, 1f, 0.4f),
            new Color(1f, 0.5f, 0.8f), new Color(0.5f, 1f, 0.9f),
        };

        public static void Build(GameObject root, int index)
        {
            Color c = CrabColors[index % CrabColors.Length];

            // Cuerpo
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0, 0.9f, 0);
            body.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            body.GetComponent<Renderer>().material.color = c;
            // Evitar que el cuerpo bloquee el CharacterController del root
            Object.Destroy(body.GetComponent<Collider>());

            // Cabeza / ojos saltones
            for (int i = -1; i <= 1; i += 2)
            {
                var stalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stalk.transform.SetParent(root.transform);
                stalk.transform.localPosition = new Vector3(i * 0.18f, 1.75f, 0.15f);
                stalk.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
                Object.Destroy(stalk.GetComponent<Collider>());

                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(root.transform);
                eye.transform.localPosition = new Vector3(i * 0.18f, 1.92f, 0.15f);
                eye.transform.localScale = Vector3.one * 0.16f;
                var er = eye.GetComponent<Renderer>();
                er.material.color = Color.white;
                Object.Destroy(eye.GetComponent<Collider>());

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.transform.SetParent(root.transform);
                pupil.transform.localPosition = new Vector3(i * 0.18f, 1.92f, 0.28f);
                pupil.transform.localScale = Vector3.one * 0.07f;
                pupil.GetComponent<Renderer>().material.color = Color.black;
                Object.Destroy(pupil.GetComponent<Collider>());
            }

            // Pinzas
            for (int i = -1; i <= 1; i += 2)
            {
                var claw = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                claw.transform.SetParent(root.transform);
                claw.transform.localPosition = new Vector3(i * 0.55f, 1.1f, 0.3f);
                claw.transform.localScale = new Vector3(0.3f, 0.22f, 0.35f);
                claw.GetComponent<Renderer>().material.color = c * 0.85f;
                Object.Destroy(claw.GetComponent<Collider>());
            }

            // Sombrero (para HatKing, desactivado por defecto)
            var hat = new GameObject("Hat");
            hat.transform.SetParent(root.transform);
            hat.transform.localPosition = new Vector3(0, 2.1f, 0);
            var crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.transform.SetParent(hat.transform);
            crown.transform.localPosition = Vector3.zero;
            crown.transform.localScale = new Vector3(0.45f, 0.3f, 0.45f);
            crown.GetComponent<Renderer>().material.color = Color.yellow;
            Object.Destroy(crown.GetComponent<Collider>());
            hat.SetActive(false);

            // Bomba (para BombTag)
            var bomb = new GameObject("Bomb");
            bomb.transform.SetParent(root.transform);
            bomb.transform.localPosition = new Vector3(0, 2.2f, 0);
            var bs = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bs.transform.SetParent(bomb.transform);
            bs.transform.localScale = Vector3.one * 0.4f;
            bs.GetComponent<Renderer>().material.color = Color.black;
            Object.Destroy(bs.GetComponent<Collider>());
            bomb.SetActive(false);
        }

        public static void SetHat(GameObject root, bool on)
        {
            var hat = root.transform.Find("Hat");
            if (hat) hat.gameObject.SetActive(on);
        }

        public static void SetBomb(GameObject root, bool on)
        {
            var b = root.transform.Find("Bomb");
            if (b) b.gameObject.SetActive(on);
        }
    }
}
