using UnityEngine;
using System.Collections;

public class ShowPhoto : MonoBehaviour
{
    public GameObject grid;
    public bool isShow;

	// Use this for initialization
	void Start ()
	{
	    isShow = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void ShowPhotoFunc()
    {
        if (isShow)
        {
            gameObject.GetComponent<TweenScale>().PlayReverse();
            gameObject.GetComponent<TweenPosition>().PlayReverse();

            gameObject.transform.SetParent(grid.transform);
        }
        else
        {
            gameObject.GetComponent<TweenPosition>().from = gameObject.transform.position;
            gameObject.GetComponent<TweenScale>().PlayForward();
            gameObject.GetComponent<TweenPosition>().PlayForward();

            gameObject.transform.SetParent(UIRoot.list[0].transform);   
        }

        isShow = !isShow;   
    }
}
