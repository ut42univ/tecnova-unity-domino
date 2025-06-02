using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public static MouseLook instance;

    // 定数
    private const float FULL_ROTATION_DEGREES = 360f;
    private const float HALF_ROTATION_MULTIPLIER = 0.5f;
    private const float SMOOTHING_FACTOR = 1f;

    [Header("Settings")]
    public Vector2 clampInDegrees = new Vector2(360, 180);
    public bool lockCursor = true;
    [Space]
    private Vector2 sensitivity = new Vector2(2, 2);
    [Space]
    public Vector2 smoothing = new Vector2(3, 3);

    [Header("First Person")]
    public GameObject characterBody;

    // 回転関連
    private Vector2 targetDirection;
    private Vector2 targetCharacterDirection;
    private Vector2 mouseAbsolute;
    private Vector2 smoothMouse;

    [HideInInspector]
    public bool scoped;

    void Start()
    {
        InitializeMouseLook();
    }

    /// <summary>
    /// MouseLookの初期化処理
    /// </summary>
    private void InitializeMouseLook()
    {
        instance = this;

        // 初期回転方向を設定
        targetDirection = transform.localRotation.eulerAngles;

        if (characterBody)
            targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;

        if (lockCursor)
            LockCursor();
    }

    /// <summary>
    /// カーソルをロックして非表示にする
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ProcessMouseInput();
        ApplyRotation();
    }

    /// <summary>
    /// マウス入力の処理とスムージング
    /// </summary>
    private void ProcessMouseInput()
    {
        // マウスの生入力を取得
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // 感度とスムージングを適用
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // スムーズな移動を計算
        smoothMouse.x = Mathf.Lerp(smoothMouse.x, mouseDelta.x, SMOOTHING_FACTOR / smoothing.x);
        smoothMouse.y = Mathf.Lerp(smoothMouse.y, mouseDelta.y, SMOOTHING_FACTOR / smoothing.y);

        // 絶対座標に加算
        mouseAbsolute += smoothMouse;

        // 回転制限を適用
        ApplyRotationClamps();
    }

    /// <summary>
    /// 回転制限を適用
    /// </summary>
    private void ApplyRotationClamps()
    {
        if (clampInDegrees.x < FULL_ROTATION_DEGREES)
            mouseAbsolute.x = Mathf.Clamp(mouseAbsolute.x, -clampInDegrees.x * HALF_ROTATION_MULTIPLIER, clampInDegrees.x * HALF_ROTATION_MULTIPLIER);

        if (clampInDegrees.y < FULL_ROTATION_DEGREES)
            mouseAbsolute.y = Mathf.Clamp(mouseAbsolute.y, -clampInDegrees.y * HALF_ROTATION_MULTIPLIER, clampInDegrees.y * HALF_ROTATION_MULTIPLIER);
    }

    /// <summary>
    /// カメラとキャラクターの回転を適用
    /// </summary>
    private void ApplyRotation()
    {
        var targetOrientation = Quaternion.Euler(targetDirection);

        // 縦回転（ピッチ）を適用
        ApplyVerticalRotation(targetOrientation);

        // 横回転（ヨー）を適用
        ApplyHorizontalRotation(targetOrientation);
    }

    /// <summary>
    /// 縦回転（ピッチ）を適用
    /// </summary>
    private void ApplyVerticalRotation(Quaternion targetOrientation)
    {
        transform.localRotation = Quaternion.AngleAxis(-mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;
    }

    /// <summary>
    /// 横回転（ヨー）を適用
    /// </summary>
    private void ApplyHorizontalRotation(Quaternion targetOrientation)
    {
        if (characterBody)
        {
            // キャラクターボディがある場合：キャラクターを回転
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);
            var yRotation = Quaternion.AngleAxis(mouseAbsolute.x, Vector3.up);
            characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
        }
        else
        {
            // キャラクターボディがない場合：カメラ自体を回転
            var yRotation = Quaternion.AngleAxis(mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }
    }
}
