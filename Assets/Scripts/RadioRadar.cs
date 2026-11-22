using UnityEngine;

namespace Tishe {
    internal sealed class RadioRadar {
        const int Resolution = 224;
        const int Diameter = 162;
        readonly Texture2D[] maps = new Texture2D[3];
        static readonly string[] Labels = { "ЖЕРТӨЛЕ", "1-ҚАБАТ", "2-ҚАБАТ" };
        const float EchoLifetime = 2.8f;
        const float SilenceInterval = 4;
        static readonly string[] Sounds = { "Тыныштық", "Сықыр", "Аяқ дыбысы" };
        static readonly Color Phosphor = new Color32(167, 220, 185, 255);
        static readonly Color SignalRed = new Color32(255, 93, 76, 255);
        Texture2D sweep, ring;
        GUIStyle label, signalLabel, probabilityLabel;
        float quietTime, echoAge = EchoLifetime;
        Vector3 echoPosition;
        string probabilityText = "HMM · Жақын: 20%";
        internal int LastSound { get; private set; } = -1;
        internal double Near { get; private set; } = .2;

        internal void Reset() {
            echoAge = EchoLifetime;
            quietTime = 0;
            LastSound = -1;
            Near = .2;
            probabilityText = "HMM · Жақын: 20%";
        }

        void Observe(int sound) {
            double previous = Near;
            Near = AiAlgorithms.UpdateNear(previous, sound);
            LastSound = sound;
            probabilityText = $"HMM · Жақын: {previous * 100:0}% → {Near * 100:0}%";
        }

        internal void Hear(int sound, Vector3 player, Vector3 keeper) {
            Observe(sound);
            quietTime = 0;
            echoPosition = keeper;
            echoAge = ShowsKeeper(player, keeper) ? 0 : EchoLifetime;
        }

        internal void Tick(float elapsed, Vector3 player, Vector3 keeper) {
            echoAge = ShowsKeeper(player, keeper) && ShowsKeeper(player, echoPosition)
                ? Mathf.Min(EchoLifetime, echoAge + elapsed) : EchoLifetime;
            quietTime += elapsed;
            while (quietTime >= SilenceInterval) {
                quietTime -= SilenceInterval;
                Observe(0);
            }
        }

        internal bool TryEcho(Vector3 player, out Vector3 position) {
            position = echoPosition;
            return echoAge < EchoLifetime && ShowsKeeper(player, position);
        }

        internal static int FloorAt(Vector3 feet) =>
            feet.y < -1.8f ? -1 : feet.y < HouseMap.Height * .5f ? 0 : 1;

        internal static bool ShowsKeeper(Vector3 player, Vector3 keeper) => FloorAt(player) == FloorAt(keeper);

        static Vector2 Center(int floor) => floor < 0 ? new Vector2(24, 9.6f) : new Vector2(floor == 0 ? 1 : 0, 0);
        static float Span(int floor) => floor < 0 ? 14f : 44f;

        // Map coordinates run east and north from the lower left, like the texture pixels.
        internal static Vector2 Project(Vector3 position, int floor) =>
            (new Vector2(position.x, position.z) - Center(floor)) / Span(floor) + Vector2.one * .5f;

        static byte Surface(int floor, float x, float z) {
            bool ramp = Mathf.Abs(z - 9.6f) <= 1.125f;
            if (floor < 0) {
                if (ramp && x >= 19.2f && x <= 22.8f) return 3;
                return (byte)(x >= 21.6f && x <= 28.8f && z >= 6f && z <= 13.2f ? 1 : 0);
            }
            if (floor == 0 && ramp && x > 13.2f && x <= 19.2f) return (byte)(x >= 15.6f ? 3 : 1);
            int col = Mathf.FloorToInt(x / HouseMap.Cell + 6.5f);
            int row = Mathf.FloorToInt(z / HouseMap.Cell + 6.5f);
            if (col == 10 && row >= 3 && row <= 9) return 2;
            return (byte)(HouseMap.Walkable(floor, row, col) ? 1 : 0);
        }

        static Texture2D BuildMap(int floor) {
            var pixels = new Color32[Resolution * Resolution];
            var surfaces = new byte[pixels.Length];
            Vector2 center = Center(floor);
            float metersPerPixel = Span(floor) / Resolution;
            float half = Resolution * .5f;
            for (int y = 0; y < Resolution; y++)
                for (int x = 0; x < Resolution; x++)
                    surfaces[y * Resolution + x] = Surface(floor, center.x + (x + .5f - half) * metersPerPixel,
                        center.y + (y + .5f - half) * metersPerPixel);

            for (int y = 0; y < Resolution; y++) {
                for (int x = 0; x < Resolution; x++) {
                    float dx = x + .5f - half, dy = y + .5f - half;
                    float radius = Mathf.Sqrt(dx * dx + dy * dy);
                    if (radius >= half) continue;
                    int i = y * Resolution + x;
                    byte surface = surfaces[i];
                    var color = new Color32(13, 22, 22, 239);
                    if (radius > half - 2) {
                        color = new Color32(148, 170, 156, (byte)(220 * Mathf.Clamp01(half - radius)));
                    } else if (surface > 0) {
                        bool edge = surfaces[i-1] == 0 || surfaces[i+1] == 0 ||
                                    surfaces[i-Resolution] == 0 || surfaces[i+Resolution] == 0;
                        bool tread = (surface == 2 && y % 5 == 0) || (surface == 3 && x % 5 == 0);
                        color = edge ? new Color32(175, 187, 173, 255) :
                                tread ? new Color32(132, 151, 139, 255) : new Color32(71, 91, 87, 255);
                    }
                    if (radius >= half - 8 && radius < half - 2) {
                        float bearing = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        float segment = Mathf.Repeat(bearing + 7.5f, 15) - 7.5f;
                        bool major = Mathf.Abs(Mathf.DeltaAngle(bearing, Mathf.Round(bearing / 90) * 90)) < 1;
                        if ((major || radius > half - 5) && Mathf.Abs(segment) < .65f)
                            color = new Color32(116, 159, 138, 255);
                    }
                    pixels[i] = color;
                }
            }
            var texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false) {
                name = "Radio / " + Labels[floor + 1], wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        static Texture2D BuildOverlay(bool sweep) {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) {
                name = sweep ? "Radio sweep" : "Radio echo", wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float dx = (x + .5f) / size * 2 - 1, dy = (y + .5f) / size * 2 - 1;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha;
                    if (sweep) {
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        float tail = Mathf.Clamp01(1 - angle / 54);
                        alpha = angle >= 0 && r < .967f ? (.48f * tail * tail + (angle < 2.4f ? .46f : 0)) * (.38f + .62f * r) : 0;
                    } else {
                        alpha = Mathf.Clamp01(1 - Mathf.Abs(r - .85f) / .11f);
                    }
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
                }
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return texture;
        }

        internal void Prepare() {
            if (sweep == null) sweep = BuildOverlay(true);
            if (ring == null) ring = BuildOverlay(false);
            for (int i = 0; i < maps.Length; i++)
                if (maps[i] == null) maps[i] = BuildMap(i - 1);
        }

        static void Dot(Vector2 uv, float size, Color color) {
            float x = uv.x * Diameter, y = (1 - uv.y) * Diameter;
            var rect = new Rect(x - size / 2, y - size / 2, size, size);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0, 0, 0, color.a), 0, size);
            rect = new Rect(rect.x + 1, rect.y + 1, size - 2, size - 2);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, size);
        }

        static void Tint(Rect rect, Texture image, Color color) {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, image);
            GUI.color = previous;
        }

        void DrawEcho(Vector2 uv) {
            float fade = Mathf.Clamp01((EchoLifetime - echoAge) / .9f);
            float pulse = Mathf.Clamp01(echoAge / .84f);
            float radius = 11 + 16 * pulse;
            float x = uv.x * Diameter, y = (1 - uv.y) * Diameter;
            var glow = SignalRed;
            glow.a = 1 - pulse;
            Tint(new Rect(x - radius, y - radius, radius * 2, radius * 2), ring, glow);
            var dot = SignalRed; dot.a = fade;
            Dot(uv, Mathf.Lerp(11, 6, echoAge / EchoLifetime), dot);
        }

        void DrawReadout(GUIStyle text) {
            if (label == null) {
                label = new GUIStyle(text) { alignment = TextAnchor.MiddleCenter, fontSize = 13, fontStyle = FontStyle.Bold };
                signalLabel = new GUIStyle(label) { fontSize = 16 };
                probabilityLabel = new GUIStyle(label) { fontSize = 16, fontStyle = FontStyle.Normal };
                label.normal.textColor = Phosphor;
                signalLabel.normal.textColor = Phosphor;
                probabilityLabel.normal.textColor = new Color32(232, 238, 222, 255);
            }
            var plate = new Rect(-24, Diameter + 3, Diameter + 48, 43);
            GUI.DrawTexture(plate, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color32(11, 21, 21, 220), 0, 6);
            GUI.Label(new Rect(plate.x, plate.y, plate.width, 22), LastSound < 0 ? "Сигнал күтілуде" : Sounds[LastSound], signalLabel);
            GUI.Label(new Rect(plate.x, plate.y + 19, plate.width, 22), probabilityText, probabilityLabel);
        }

        internal void Draw(Vector3 player, GUIStyle text, float sweepAngle) {
            if (Event.current.type != EventType.Repaint) return;
            int floor = FloorAt(player);
            float scale = Mathf.Min(Screen.width / 1360f, Screen.height / 800f);
            var previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.width - (38 + Diameter) * scale, 22 * scale),
                Quaternion.identity, new Vector3(scale, scale, 1));
            try {
                var disc = new Rect(0, 0, Diameter, Diameter);
                GUI.DrawTexture(disc, maps[floor + 1]);
                var mapMatrix = GUI.matrix;
                Vector3 pivot = disc.center;
                // Rotate in dial coordinates before applying the HUD's translation and scale.
                GUI.matrix = mapMatrix * Matrix4x4.TRS(pivot, Quaternion.Euler(0, 0, sweepAngle), Vector3.one)
                    * Matrix4x4.Translate(-pivot);
                Tint(disc, sweep, Phosphor);
                GUI.matrix = mapMatrix;
                if (TryEcho(player, out var position)) DrawEcho(Project(position, floor));
                Dot(Project(player, floor), 9, new Color32(99, 255, 140, 255));
                DrawReadout(text);
                var caption = new Rect(Diameter / 2 - 45, Diameter - 24, 90, 22);
                GUI.Label(caption, Labels[floor + 1], label);
            } finally {
                GUI.matrix = previous;
            }
        }

        internal void Dispose() {
            foreach (var texture in maps)
                if (texture != null) Object.Destroy(texture);
            if (sweep != null) Object.Destroy(sweep);
            if (ring != null) Object.Destroy(ring);
        }
    }
}
