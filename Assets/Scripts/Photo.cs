using System;
using UnityEngine;
using System.Collections;
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
    public UserShar user;

    public static Photo currentPhotoShowing;

    private TweenScale _tweenScale;
    private TweenPosition _tweenPosition;
    private Vector3 _lastPos;

    private void OnEnable()
    {
        foreach (var tap in GetComponents<TapGesture>())
        {
            tap.Tapped += tappedHandler;
        }
    }

    private void OnDisable()
    {
        foreach (var tap in GetComponents<TapGesture>())
        {
            tap.Tapped -= tappedHandler;
        }
    }

    // Use this for initialization
    void Start ()
	{
	    isShow = false;

        _tweenScale = gameObject.GetComponent<TweenScale>();
        _tweenPosition = gameObject.GetComponent<TweenPosition>();

	}
	
    public void ShowPhoto()
    {
        if (isShow)
        {
            _tweenScale.PlayReverse();
            _tweenPosition.PlayReverse();
            
            //gameObject.transform.SetParent(grid.transform);

            currentPhotoShowing = null;

            isShow = !isShow;

        }
        else if (currentPhotoShowing == null)
        {
            _tweenPosition.from = gameObject.transform.localPosition;

            _tweenScale.PlayForward();
            _tweenPosition.PlayForward();

            gameObject.transform.SetParent(UIRoot.list[0].transform);

            InstaGallery.current.BringToFront(gameObject);

            currentPhotoShowing = this;

            isShow = !isShow;
        }

    }

    private void tappedHandler(object sender, EventArgs eventArgs)
    {
        var tap = sender as TapGesture;

        /*ITouchHit hit;
        tap.GetTargetHitResult(out hit);
        var hit3d = hit as ITouchHit3D;
        if (hit3d == null) return;*/

        switch (tap.NumberOfTapsRequired)
        {
            case 1:
                // our single tap gesture
                ShowPhoto();
                break;
            case 2:
                // our double tap gesture
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
}
