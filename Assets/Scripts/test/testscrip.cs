using UnityEngine;
using UnityEngine.Profiling;

public class testscrip : MonoBehaviour
{
     private float deltaTime = 0.0f;

        void Update()
        {
            // FPS 계산: Time.deltaTime을 누적해서 프레임 간 시간 평균을 구함
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            // 스타일 설정 (텍스트 크기, 색상 등)
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.black;

            // FPS 계산
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string fpsText = string.Format("FPS: {0:0.} ({1:0.0} ms)", fps, msec);

            // 메모리 사용량 계산 (Unity Profiler 사용)
            long memory = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024; // MB 단위로 변환
            string memoryText = string.Format("Memory: {0} MB", memory);

            // 화면에 표시
            GUI.Label(new Rect(10, 10, 300, 20), fpsText, style);
            GUI.Label(new Rect(10, 40, 300, 20), memoryText, style);
        }
    
}
