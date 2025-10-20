using UnityEngine;
using UnityEngine.UI;

public class MultiplayerKeyUIManager : MonoBehaviour
{
    public Button w;
    public Button a;
    public Button s;
    public Button d;
    public Button WASDShift;

    public Button up;
    public Button down;
    public Button left;
    public Button right;
    public Button ArrowShift;

    private Color WASDColor = new Color(0.9215686f, 0.2078431f, 0.2039216f);
    private Color WASDpressedColor = new Color(0.5754717f, 0.2272161f, 0.2253026f);

    private Color ArrowColor = new Color(0.9607843f, 0.8705882f, 0.007843138f);
    private Color ArrowpressedColor = new Color(0.5849056f, 0.548085f, 0.1958882f);

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // simulate press
            if (w != null) w.GetComponent<Image>().color = WASDpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            // release visual
            if (w != null) w.GetComponent<Image>().color = WASDColor;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // simulate press
            if (a != null) a.GetComponent<Image>().color = WASDpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            // release visual
            if (a != null) a.GetComponent<Image>().color = WASDColor;
        }

        if (Input.GetKey(KeyCode.S))
        {
            // simulate press
            if (s != null) s.GetComponent<Image>().color = WASDpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            // release visual
            if (s != null) s.GetComponent<Image>().color = WASDColor;
        }

        if (Input.GetKey(KeyCode.D))
        {
            // simulate press
            if (d != null) d.GetComponent<Image>().color = WASDpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            // release visual
            if (d != null) d.GetComponent<Image>().color = WASDColor;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            // simulate press
            if (WASDShift != null) WASDShift.GetComponent<Image>().color = WASDpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            // release visual
            if (WASDShift != null) WASDShift.GetComponent<Image>().color = WASDColor;
        }


        if (Input.GetKey(KeyCode.UpArrow))
        {
            // simulate press
            if (up != null) up.GetComponent<Image>().color = ArrowpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            // release visual
            if (up != null) up.GetComponent<Image>().color = ArrowColor;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            // simulate press
            if (down != null) down.GetComponent<Image>().color = ArrowpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            // release visual
            if (down != null) down.GetComponent<Image>().color = ArrowColor;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // simulate press
            if (left != null) left.GetComponent<Image>().color = ArrowpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            // release visual
            if (left != null) left.GetComponent<Image>().color = ArrowColor;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            // simulate press
            if (right != null) right.GetComponent<Image>().color = ArrowpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            // release visual
            if (right != null) right.GetComponent<Image>().color = ArrowColor;
        }

        if (Input.GetKey(KeyCode.RightShift))
        {
            // simulate press
            if (ArrowShift != null) ArrowShift.GetComponent<Image>().color = ArrowpressedColor;
        }
        if (Input.GetKeyUp(KeyCode.RightShift))
        {
            // release visual
            if (ArrowShift != null) ArrowShift.GetComponent<Image>().color = ArrowColor;
        }
    }
}
