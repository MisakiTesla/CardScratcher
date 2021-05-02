using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class CardScratcher : MonoBehaviour
{
    public Canvas canvas;
    public Material mat;
    public RectTransform mask;    
    public float scratchRadius;

    private Vector2 _lastTouchPos;
    private bool _isFirstTouch = true;
    private bool _isTouching = false;

    private float _progress = 1f;
    private bool _isInAera = false;
    public float Progress => _progress;

    public event Action OnScratchStart; 
    // public event Action<float> OnProcessChange; 
    private RenderTexture _texture;
    private RenderTexture _tmptexture;//doublebuffer

    private bool doubleBufferFlag;
    private void Awake()
    {
        _texture = RenderTexture.GetTemporary((int)mask.rect.width,(int)mask.rect.height,24,GraphicsFormat.R8G8B8A8_UNorm);
        _tmptexture = RenderTexture.GetTemporary((int)mask.rect.width,(int)mask.rect.height,24,GraphicsFormat.R8G8B8A8_UNorm);
        mask.GetComponent<RawImage>().texture = _texture;
    }

    /// <summary>
    /// 计算刮卡进度
    /// 计算刮卡进度
    /// </summary>
    private void CalcProgress()
    {
        var texture2D = RenderTexture2Texture2D(_texture);
        var colorArray = texture2D.GetPixels(texture2D.mipmapCount - 1);
        if (colorArray.Length == 1)
        {
            _progress = 1 - colorArray[0].a;
        }
        else
        {
            _progress = 1;
        }
        Destroy(texture2D);
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            OnMouseDown(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isTouching = false;
        }
        
    }

    private void OnMouseDown(Vector2 touchPos)
    {
        if(RectTransformUtility.RectangleContainsScreenPoint(mask,touchPos))
        {
            Vector2 localPosInMask = Vector2.zero;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mask, touchPos, null, out localPosInMask);
            }
            else if(canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mask, touchPos, canvas.worldCamera, out localPosInMask);
            }
            

            if (_isFirstTouch)
            {
                _isFirstTouch = false;
                OnScratchStart?.Invoke();
                mat.SetFloat("_HoleRadius", scratchRadius);
            }



            localPosInMask.x = Remap(-mask.rect.width/2,mask.rect.width/2,0,1, localPosInMask.x);
            localPosInMask.y = Remap(-mask.rect.height/2,mask.rect.height/2,0,1, localPosInMask.y);
            Debug.Log($"touchPos:{touchPos},localPos{localPosInMask}");

            if (!_isTouching)
            {
                _isTouching = true;
                _lastTouchPos = localPosInMask;
            }
            // mat.SetVector("_MousePos", new Vector4(_lastTouchPos.x, _lastTouchPos.y, touchPos.x, touchPos.y));
            mat.SetVector("_MousePos", new Vector4(_lastTouchPos.x, _lastTouchPos.y, localPosInMask.x, localPosInMask.y));

            // _lastTouchPos = touchPos;
            _lastTouchPos = localPosInMask;
            if (doubleBufferFlag)
            {
                Graphics.Blit(_tmptexture, _texture, mat, 0);
            }
            else
            {
                Graphics.Blit(_texture, _tmptexture, mat, 0);
            }
            doubleBufferFlag = !doubleBufferFlag;

        }
        

        StartCoroutine(OnPostRender());
    }

    private IEnumerator OnPostRender()
    {
        //TODO 间隔调用
        // if (_isTouching)
        {
            yield return new WaitForEndOfFrame();
            Debug.Log($"ClearProgress:{_progress}");
            CalcProgress();        
        }

    }

    public Texture2D RenderTexture2Texture2D(RenderTexture rt)
    {
        RenderTexture preRT = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, true);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = preRT;
        return tex;
    }

    public void Clear()
    {
        Debug.Log($"=======Clear");
        
        Graphics.Blit(null, _texture, mat, 1);//清空RT
        Graphics.Blit(null, _tmptexture, mat, 1);//清空RT
        _isTouching = false;
        _isFirstTouch = true;
    }

    private float Remap(float oldMin, float oldMax, float newMin, float newMax, float oldValue)
    {
        return ((oldValue - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
    }

    private void OnDestroy()
    {
        _texture.Release();
        _tmptexture.Release();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(new Vector2(100,Screen.height - 100), new Vector2(200, 100)), "ClearToBlack"))
        {
            Clear();
        }
        if (GUI.Button(new Rect(new Vector2(300,Screen.height - 100), new Vector2(200, 100)), "ClearToWhite"))
        {
            Graphics.Blit(null, _texture, mat, 2);//清空RT
            Graphics.Blit(null, _tmptexture, mat, 2);//清空RT
            _isTouching = false;
            _isFirstTouch = true;
        }

    }
}
