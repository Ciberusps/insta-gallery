using System;
using UnityEngine;
using System.Collections;
using System.Threading;
using DG.Tweening;
using TouchScript.Gestures;
using TouchScript.Hit;

public class Photo : MonoBehaviour
{
    public GameObject grid;
    public bool isShow;

    //Photo properties
    public int createdTime;
    public string link;
    public string id;
    public float allPhotoesScale;
    public float MaxHeight = 2;
    public float JumpSpeed = 5;
    
    public UserShar user;
    public static Photo currentPhotoShowing;
    public Transform target;

    
    private bool selected = false;
    private float startHeight;
    private TweenScale _tweenScale;
    private TweenPosition _tweenPosition;
    private Vector3 _lastPos;

    private void OnEnable()
    {
        foreach (var tap in GetComponents<TapGesture>())
        {
            tap.Tapped += tappedHandler;
        }

        isShow = false;

        _tweenScale = gameObject.GetComponent<TweenScale>();
        _tweenPosition = gameObject.GetComponent<TweenPosition>();

        ResetPhoto();
    }

    private void OnDisable()
    {
        foreach (var tap in GetComponents<TapGesture>())
        {
            tap.Tapped -= tappedHandler;
        }
    }

    // Use this for initialization
    void Start()
	{

        /*isShow = false;

        _tweenScale = gameObject.GetComponent<TweenScale>();
        _tweenPosition = gameObject.GetComponent<TweenPosition>();*/


        startHeight = transform.localPosition.z;

        GetComponent<PressGesture>().Pressed += pressedHandler;
        GetComponent<ReleaseGesture>().Released += releasedHandler;
    }

 

    public void ShowPhoto()
    {
        if (isShow)
        {
            /*_tweenScale.PlayReverse();
            _tweenPosition.PlayReverse();*/
            print(DOTween.KillAll());

            gameObject.transform.DOScale(new Vector3(-0.5f, -0.5f, -0.5f), 1).SetRelative().SetLoops(1).SetAutoKill();
            //gameObject.transform.DOLocalMove(_lastPos, 2);

            //gameObject.transform.SetParent(grid.transform);

            currentPhotoShowing = null;

            isShow = !isShow;

        }
        else if (currentPhotoShowing == null)
        {
            _lastPos = transform.localPosition;
            //_tweenPosition.from = gameObject.transform.localPosition;
            print(DOTween.KillAll());

            gameObject.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 1).SetRelative().SetLoops(1).SetAutoKill();
            //gameObject.transform.DOLocalMove(Vector3.zero, 2);
            /*_tweenScale.PlayForward();
            _tweenPosition.PlayForward();*/

            //gameObject.transform.SetParent(target);

            InstaGallery.current.BringToFront(gameObject);

            currentPhotoShowing = this;

            isShow = !isShow;
        }

    }

    private void tappedHandler(object sender, EventArgs eventArgs)
    {
        var tap = sender as TapGesture;
        switch (tap.NumberOfTapsRequired)
        {
            case 1:
                // our single tap gesture
                break;
            case 2:
                // our double tap gesture
                ShowPhoto();
                break;
        }
    }

    public struct UserShar
    {
        public string name;
        public string profilePicture;
        public string fullName;
        public string id;
    }

    public void ResetPhoto()
    {
        isShow = false;

        //InstaGallery.current.BringToPhotoStation(gameObject.transform.parent.gameObject);
        gameObject.transform.localScale = new Vector3(allPhotoesScale, allPhotoesScale, allPhotoesScale);
        gameObject.GetComponent<UITexture>().mainTexture = null;
    }

    private void Update()
    {
        var targetY = startHeight;
        if (selected) targetY = startHeight + MaxHeight;
        var newPosition = transform.localPosition;
        newPosition.z = Mathf.Lerp(transform.localPosition.z, targetY, Time.deltaTime * JumpSpeed);

       

        /*if(newPosition.x <= -550) newPosition.x = -550;
        if (newPosition.x >= 550) newPosition.x = 550;
        if (newPosition.y >= 200) newPosition.y = 200;
        if (newPosition.y <= -200) newPosition.y = -200;*/

        transform.localPosition = newPosition;
    }

    private void releasedHandler(object sender, EventArgs eventArgs)
    {
        selected = false;
    }

    private void pressedHandler(object sender, EventArgs eventArgs)
    {
        selected = true;
        InstaGallery.current.BringToFront(gameObject);
    }
}
