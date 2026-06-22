using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Tạo hiệu ứng các ký tự chữ Loading (bao gồm cả dấu chấm) nảy lên xuống nhịp nhàng như làn sóng.
/// Sử dụng trực tiếp cấu trúc Mesh của TextMeshPro để đạt hiệu năng tối đa mà không cần tạo nhiều GameObject.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class LoadingTextAnimator : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private string baseText = "";
    private Coroutine dotsCoroutine;

    [Header("Cấu hình Nảy Chữ (Bounce Wave)")]
    [Tooltip("Độ cao nảy lên xuống của chữ")]
    [SerializeField] private float bounceHeight = 8f;
    [Tooltip("Tốc độ nảy")]
    [SerializeField] private float bounceSpeed = 6f;
    [Tooltip("Độ trễ nhịp nảy giữa các ký tự kế cận (tạo hiệu ứng lượn sóng)")]
    [SerializeField] private float characterWaveDelay = 0.25f;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Giữ nguyên toàn bộ text thiết lập sẵn từ Editor hoặc code truyền vào (ví dụ: "Loading..." đã có sẵn 3 chấm)
        baseText = textComponent.text;
    }

    private void OnDisable()
    {
        // Không cần coroutine nữa
    }

    private void Update()
    {
        // Thực hiện biến đổi Mesh của TextMeshPro để làm các ký tự dấu chấm nảy lên xuống
        AnimateTextMesh();
    }

    /// <summary>
    /// Cho phép cập nhật nội dung chữ gốc từ bên ngoài
    /// </summary>
    public void SetBaseText(string newText)
    {
        baseText = newText;
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
    }

    /// <summary>
    /// Làm nảy từng dấu chấm độc lập bằng cách thay đổi tọa độ Vertex của Mesh
    /// </summary>
    private void AnimateTextMesh()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        int characterCount = textInfo.characterCount;
        if (characterCount == 0) return;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Bỏ qua nếu ký tự không hiển thị (khoảng trắng, xuống dòng...)
            if (!charInfo.isVisible) continue;

            // CHỈ LÀM NẢY DẤU CHẤM: Nếu ký tự không phải là dấu chấm '.', giữ nguyên vị trí đứng yên
            if (charInfo.character != '.') continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

            // Tính toán vị trí nảy dựa trên hàm Sin và thời gian thực, có độ trễ giữa các dấu chấm để tạo nhịp nảy nối đuôi
            float offset = Mathf.Sin(Time.time * bounceSpeed + i * characterWaveDelay) * bounceHeight;
            Vector3 translation = new Vector3(0, offset, 0);

            // Cập nhật vị trí của 4 đỉnh tạo nên dấu chấm
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + translation;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + translation;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + translation;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + translation;
        }

        // Đẩy dữ liệu Mesh mới lên card màn hình để vẽ lại chữ đã thay đổi tọa độ
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
