using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public class PuzzleTileView : MonoBehaviour, IPointerClickHandler
{
    public enum TileShape
    {
        Empty,
        Straight,
        Corner,
        TShape,
        Cross,
        Source,
        Target
    }

    [Header("references")]
    [SerializeField] private RectTransform tileRoot;
    [SerializeField] private GameObject centerNode;
    [SerializeField] private GameObject armTop;
    [SerializeField] private GameObject armRight;
    [SerializeField] private GameObject armBottom;
    [SerializeField] private GameObject armLeft;
    [SerializeField] private GameObject sourceLabel;
    [SerializeField] private GameObject targetLabel;

    [SerializeField] private TextMeshProUGUI targetLabelText;

    [SerializeField] private UnityEngine.UI.Image backgroundImage;
    [SerializeField] private UnityEngine.UI.Image centerImage;
    [SerializeField] private UnityEngine.UI.Image armTopImage;
    [SerializeField] private UnityEngine.UI.Image armRightImage;
    [SerializeField] private UnityEngine.UI.Image armBottomImage;
    [SerializeField] private UnityEngine.UI.Image armLeftImage;

    [SerializeField] private UnityEngine.UI.Image centerFill;
    [SerializeField] private UnityEngine.UI.Image armTopFill;
    [SerializeField] private UnityEngine.UI.Image armRightFill;
    [SerializeField] private UnityEngine.UI.Image armBottomFill;
    [SerializeField] private UnityEngine.UI.Image armLeftFill;

    [Header("tile setup")]
    [SerializeField] private TileShape tileShape = TileShape.Corner;
    [SerializeField] private int currentRotation = 0;

    [Header("colors")]
    [SerializeField] private Color normalBackgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    [SerializeField] private Color poweredBackgroundColor = new Color(0.2f, 1f, 0.3f, 1f);

    [SerializeField] private Color normalCenterColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color poweredCenterColor = new Color(0.1f, 1f, 0.2f, 1f);

    [SerializeField] private Color armNormalColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color armPoweredColor = new Color(0.1f, 1f, 0.2f, 1f);

    [SerializeField] private Color fillOffColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color fillOnColor = new Color(0.396f, 0.941f, 0.008f, 1f);

    [SerializeField] private Color sourceOnColor = new Color(0.396f, 0.941f, 0.008f, 1f);
    [SerializeField] private Color targetOffOutlineColor = new Color(1f, 0.55f, 0f, 1f);
    [SerializeField] private Color successColor = new Color(0.396f, 0.941f, 0.008f, 1f);
    [SerializeField] private Color failureColor = new Color(1f, 0.1f, 0.1f, 1f);

    private bool isPowered = false;
    private bool isSuccessState = false;
    private bool isFailureFlash = false;

    public TileShape Shape => tileShape;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isRotatable)
            return;

        RotateClockwise();
    }

    public void RotateClockwise()
    {
        currentRotation = (currentRotation + 1) % 4;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        if (tileRoot == null)
            tileRoot = GetComponent<RectTransform>();

        tileRoot.localEulerAngles = new Vector3(0f, 0f, -90f * currentRotation);
    }

    private void UpdateVisuals()
    {
        if (sourceLabel != null) sourceLabel.SetActive(false);
        if (targetLabel != null) targetLabel.SetActive(false);

        if (armTop == null || armRight == null || armBottom == null || armLeft == null)
            return;

        armTop.SetActive(false);
        armRight.SetActive(false);
        armBottom.SetActive(false);
        armLeft.SetActive(false);

        if (centerNode != null)
            centerNode.SetActive(tileShape != TileShape.Empty);

        switch (tileShape)
        {
            case TileShape.Empty:
                break;

            case TileShape.Straight:
                armTop.SetActive(true);
                armBottom.SetActive(true);
                break;

            case TileShape.Corner:
                armTop.SetActive(true);
                armRight.SetActive(true);
                break;

            case TileShape.TShape:
                armTop.SetActive(true);
                armRight.SetActive(true);
                armLeft.SetActive(true);
                break;

            case TileShape.Cross:
                armTop.SetActive(true);
                armRight.SetActive(true);
                armBottom.SetActive(true);
                armLeft.SetActive(true);
                break;

            case TileShape.Source:
                armRight.SetActive(true);
                armLeft.SetActive(true);
                if (sourceLabel != null) sourceLabel.SetActive(true);
                break;

            case TileShape.Target:
                armRight.SetActive(true);
                armLeft.SetActive(true);
                if (targetLabel != null) targetLabel.SetActive(true);
                break;
        }
    }

    private void Awake()
    {
        if (tileRoot == null)
            tileRoot = GetComponent<RectTransform>();

        UpdateVisuals();
        ApplyRotation();
        UpdateColors();
    }

    private void OnValidate()
    {
        if (tileRoot == null)
            tileRoot = GetComponent<RectTransform>();

        UpdateVisuals();
        ApplyRotation();
        UpdateColors();
    }

    private void Reset()
    {
        tileRoot = GetComponent<RectTransform>();
    }

    public void SetShape(TileShape newShape)
    {
        tileShape = newShape;
        UpdateVisuals();
    }

    public void SetRotation(int rotation)
    {
        currentRotation = ((rotation % 4) + 4) % 4;
        ApplyRotation();
    }

    public bool[] GetConnections()
    {

        bool[] connections = new bool[4];

        switch (tileShape)
        {
            case TileShape.Empty:
                return connections;

            case TileShape.Straight:
                connections[0] = true;
                connections[2] = true;
                break;

            case TileShape.Corner:
                connections[0] = true;
                connections[1] = true;
                break;

            case TileShape.TShape:
                connections[0] = true;
                connections[1] = true;
                connections[3] = true;
                break;

            case TileShape.Cross:
                connections[0] = true;
                connections[1] = true;
                connections[2] = true;
                connections[3] = true;
                break;

            case TileShape.Source:
                connections[1] = true; // Right
                break;

            case TileShape.Target:
                connections[3] = true; // Left
                break;
        }

        return RotateConnections(connections, currentRotation);
    }

    private bool[] RotateConnections(bool[] baseConnections, int rotation)
    {
        bool[] result = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            result[(i + rotation) % 4] = baseConnections[i];
        }

        return result;
    }

    public bool HasConnection(Direction dir)
    {
        return GetConnections()[(int)dir];
    }

    public void SetPowered(bool value)
    {
        isPowered = value;
        UpdateColors();
    }

    private void UpdateColors()
    {
        Color green = fillOnColor;
        Color red = failureColor;
        Color black = fillOffColor;

        // outline przy błędzie czerwone wszędzie, normalnie zielone
        Color outlineColor = isFailureFlash ? red : green;

        // outline wszystkich elementów
        SetOutline(centerImage, outlineColor);
        SetOutline(armTopImage, outlineColor);
        SetOutline(armRightImage, outlineColor);
        SetOutline(armBottomImage, outlineColor);
        SetOutline(armLeftImage, outlineColor);

        switch (tileShape)
        {
            case TileShape.Source:
                // środek source zawsze aktywny
                SetFill(centerFill, isFailureFlash ? red : green);

                // lewe ramię source = zewnętrzne, zawsze aktywne
                SetFill(armLeftFill, isFailureFlash ? red : green);

                // prawe ramię source = do planszy, świeci tylko przy przepływie
                SetFill(armRightFill, isPowered ? (isFailureFlash ? red : green) : black);

                SetFill(armTopFill, black);
                SetFill(armBottomFill, black);
                break;

            case TileShape.Target:
                if (isSuccessState || isPowered)
                {
                    // środek targetu robi się zielony po dojściu prądu
                    SetFill(centerFill, isFailureFlash ? red : green);

                    // lewe ramię targetu = od planszy
                    SetFill(armLeftFill, isFailureFlash ? red : green);

                    // prawe ramię targetu = zewnętrzne
                    SetFill(armRightFill, isFailureFlash ? red : green);
                }
                else
                {
                    SetFill(centerFill, black);
                    SetFill(armLeftFill, black);
                    SetFill(armRightFill, black);
                }

                SetFill(armTopFill, black);
                SetFill(armBottomFill, black);

                if (targetLabelText != null)
                {
                    if (isFailureFlash)
                        targetLabelText.color = red;
                    else if (isSuccessState)
                        targetLabelText.color = black;
                    else
                        targetLabelText.color = green;
                }
                break;

            case TileShape.Empty:
                SetFill(centerFill, black);
                SetFill(armTopFill, black);
                SetFill(armRightFill, black);
                SetFill(armBottomFill, black);
                SetFill(armLeftFill, black);
                break;

            default:
                // zwykłe kafelki: fill czerwony przy błędzie tylko tam, gdzie był prąd
                Color flowColor = isPowered ? (isFailureFlash ? red : green) : black;

                SetFill(centerFill, flowColor);
                SetFill(armTopFill, flowColor);
                SetFill(armRightFill, flowColor);
                SetFill(armBottomFill, flowColor);
                SetFill(armLeftFill, flowColor);
                break;
        }
    }

    private void SetOutline(UnityEngine.UI.Image img, Color color)
    {
        if (img == null)
            return;

        img.color = color;
    }

    private void SetFill(UnityEngine.UI.Image img, Color color)
    {
        if (img == null)
            return;

        img.color = color;
    }

    public void SetSuccessState(bool value)
    {
        isSuccessState = value;
        UpdateColors();
    }

    public void SetFailureFlash(bool value)
    {
        isFailureFlash = value;
        UpdateColors();
    }

    private bool isRotatable = true;

    public void SetRotatable(bool value)
    {
        isRotatable = value;
    }
}