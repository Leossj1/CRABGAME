using UnityEngine;
using CrabPrototype.Core;

namespace CrabPrototype.Player
{
    /// <summary>
    /// Jugador estilo Crab Game: primera persona + avatar visible para otros,
    /// movimiento arcade, salto, empujón (E / click), agarrar (F), golpe con palo.
    /// Físicas de knockback fuertes para caos divertido.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CrabPlayer : MonoBehaviour
    {
        [System.Serializable]
        public class PlayerRef
        {
            public int id;
            public string displayName;
            public CrabPlayer controller;
            public bool alive = true;
            public bool isBot;
            public bool hasHat;
            public bool hasBomb;
            public int stickHits;
        }

        [Header("Movimiento")]
        public float walkSpeed = 6f;
        public float sprintMult = 1.5f;
        public float jumpForce = 7f;
        public float gravity = -20f;
        public float mouseSens = 2.2f;

        [Header("Interacción")]
        public float pushForce = 12f;
        public float pushRadius = 2.6f;
        public float pushCooldown = 0.6f;
        public float grabDistance = 2.2f;

        public PlayerRef data;
        CharacterController cc;
        Transform cam;
        float vertVel;
        float pitch;
        float lastPush;
        CrabPlayer grabbedBy;
        Rigidbody grabbedBody;

        // Stick (modo StickFight)
        public GameObject stickVisual;
        public bool hasStick;

        public static GameObject SpawnPrefab(Vector3 pos, Quaternion rot, Transform parent, int index, bool isBot)
        {
            var go = new GameObject(isBot ? $"Player_Bot_{index}" : "Player_Local");
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.rotation = rot;
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.7f; cc.radius = 0.4f; cc.center = new Vector3(0, 0.85f, 0);
            var p = go.AddComponent<CrabPlayer>();
            CrabAvatarBuilder.Build(go, index);
            p.hasStick = false;
            // Cámara solo para local
            if (!isBot)
            {
                var camGo = new GameObject("FPSCam");
                camGo.transform.SetParent(go.transform);
                camGo.transform.localPosition = new Vector3(0, 1.6f, 0);
                var camera = camGo.AddComponent<Camera>();
                camera.fieldOfView = 75;
                camGo.AddComponent<AudioListener>();
            }
            return go;
        }

        public void Bind(PlayerRef pref)
        {
            data = pref;
            cc = GetComponent<CharacterController>();
            cam = transform.Find("FPSCam");
            Cursor.lockState = data.isBot ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = data.isBot;
        }

        void Update()
        {
            if (data == null || !data.alive) return;
            if (data.isBot) return; // bots los mueve SimpleBot

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool sprint = Input.GetKey(KeyCode.LeftShift);
            Move(h, v, sprint);

            if (Input.GetButtonDown("Jump") && cc.isGrounded)
                vertVel = jumpForce;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                TryPush();
            if (Input.GetKeyDown(KeyCode.F))
                TryGrabToggle();
            if (hasStick && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.G)))
                TryStickHit();
        }

        public void Move(float h, float v, bool sprint)
        {
            if (cc == null) cc = GetComponent<CharacterController>();

            // Mouse look solo local con cámara
            if (cam != null)
            {
                float mx = Input.GetAxis("Mouse X") * mouseSens;
                float my = Input.GetAxis("Mouse Y") * mouseSens;
                transform.Rotate(Vector3.up * mx);
                pitch = Mathf.Clamp(pitch - my, -80, 80);
                cam.localRotation = Quaternion.Euler(pitch, 0, 0);
            }

            float speed = walkSpeed * (sprint ? sprintMult : 1f);
            Vector3 dir = (transform.forward * v + transform.right * h);
            if (dir.magnitude > 1) dir.Normalize();

            if (cc.isGrounded && vertVel < 0) vertVel = -2f;
            vertVel += gravity * Time.deltaTime;

            cc.Move((dir * speed + Vector3.up * vertVel) * Time.deltaTime);
        }

        // Llamado por bots / red para mover sin input
        public void MoveExternal(Vector3 worldDir, bool jump)
        {
            if (cc == null) return;
            if (worldDir.magnitude > 1) worldDir.Normalize();
            if (cc.isGrounded && vertVel < 0) vertVel = -2f;
            if (jump && cc.isGrounded) vertVel = jumpForce;
            vertVel += gravity * Time.deltaTime;
            if (worldDir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(new Vector3(worldDir.x, 0, worldDir.z)), 10 * Time.deltaTime);
            cc.Move((worldDir * walkSpeed + Vector3.up * vertVel) * Time.deltaTime);
        }

        public void TryPush()
        {
            if (Time.time - lastPush < pushCooldown) return;
            lastPush = Time.time;
            // Empujón con knockback: esencia Crab Game
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.2f, pushRadius);
            foreach (var h in hits)
            {
                if (h.gameObject == gameObject) continue;
                var other = h.GetComponentInParent<CrabPlayer>();
                if (other == null || other == this) continue;
                Vector3 dir = (other.transform.position - transform.position).normalized + Vector3.up * 0.35f;
                other.ApplyKnockback(dir.normalized * pushForce, this);
                // Pasar bomba / robar sombrero se gestiona en los modos vía eventos
                GameManager.Instance?.ActiveMode?.SendMessage("OnPushLanded", new PushInfo { from = this, to = other }, SendMessageOptions.DontRequireReceiver);
            }
            // Feedback visual simple
            transform.localScale = Vector3.one * 1.05f;
            Invoke(nameof(ResetScale), 0.1f);
        }

        void ResetScale() => transform.localScale = Vector3.one;

        public void ApplyKnockback(Vector3 force, CrabPlayer from)
        {
            StartCoroutine(KnockbackRoutine(force));
        }

        System.Collections.IEnumerator KnockbackRoutine(Vector3 force)
        {
            float t = 0.25f;
            // Sacamos del CharacterController moviéndolo directamente (arcade)
            while (t > 0)
            {
                cc.Move(force * Time.deltaTime);
                force = Vector3.Lerp(force, Vector3.zero, 8 * Time.deltaTime);
                t -= Time.deltaTime;
                yield return null;
            }
        }

        // Agarre simple: ralentiza al agarrado
        CrabPlayer grabbed;
        public void TryGrabToggle()
        {
            if (grabbed != null) { grabbed.grabbedBy = null; grabbed = null; return; }
            Collider[] hits = Physics.OverlapSphere(transform.position, grabDistance);
            foreach (var h in hits)
            {
                var o = h.GetComponentInParent<CrabPlayer>();
                if (o != null && o != this) { grabbed = o; o.grabbedBy = this; break; }
            }
        }

        void LateUpdate()
        {
            // Si estoy agarrado, me arrastra un poco
            if (grabbedBy != null)
            {
                Vector3 to = (grabbedBy.transform.position - transform.position);
                if (to.magnitude > 1.2f) cc?.Move(to.normalized * 3f * Time.deltaTime);
            }
        }

        public void GiveStick()
        {
            hasStick = true;
            if (stickVisual == null)
            {
                stickVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stickVisual.transform.SetParent(transform);
                stickVisual.transform.localPosition = new Vector3(0.5f, 1.2f, 0.5f);
                stickVisual.transform.localScale = new Vector3(0.08f, 1.2f, 0.08f);
                stickVisual.transform.rotation = Quaternion.Euler(0, 0, 90);
                var r = stickVisual.GetComponent<Renderer>();
                r.material.color = new Color(0.6f, 0.35f, 0.15f);
            }
            stickVisual.SetActive(true);
        }

        public void TryStickHit()
        {
            if (!hasStick) return;
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.6f, 2.2f);
            foreach (var h in hits)
            {
                var o = h.GetComponentInParent<CrabPlayer>();
                if (o == null || o == this) continue;
                Vector3 dir = (o.transform.position - transform.position).normalized + Vector3.up * 0.5f;
                o.ApplyKnockback(dir.normalized * (pushForce * 1.8f), this);
                data.stickHits++;
            }
        }

        public void RespawnOrSpectate()
        {
            // En prototipo: respawn a los 3s si el modo es por puntos, si no queda especteando
            StartCoroutine(RespawnRoutine());
        }

        System.Collections.IEnumerator RespawnRoutine()
        {
            // Desactivar control 3s
            bool wasAlive = false;
            yield return new WaitForSeconds(3f);
            var gm = GameManager.Instance;
            if (gm == null) yield break;
            // Si el modo no es last-man-standing, reaparece
            if (!gm.ActiveMode.IsLastManStanding)
            {
                transform.position = gm.ActiveMode.GetSpawnPoint(data.id, 8) + Vector3.up * 2;
                vertVel = 0;
                data.alive = true;
            }
        }

        public struct PushInfo
        {
            public CrabPlayer from;
            public CrabPlayer to;
        }
    }
}
