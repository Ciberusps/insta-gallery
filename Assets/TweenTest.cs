using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TweenTest : MonoBehaviour
{
    public float duration;
	// Use this for initialization
	void Start ()
	{
	   

        Sequence s = DOTween.Sequence();

        RandomPosition(s);

        s.Append(
	        gameObject.transform.DOLocalMove(new Vector3(Random.Range(-550f, 550f), Random.Range(-250f, 250f), 0f), duration)
	            .SetEase(Ease.OutBack));
	    s.Insert(0,
	        transform.DORotate(new Vector3(Random.Range(5, 15), 0, Random.Range(5, 15)), duration/2).SetEase(Ease.InQuad).SetLoops(2, LoopType.Yoyo));
        //s.Insert(2,
        //  transform.DORotate(new Vector3(0, 0, 0), duration/2).SetEase(Ease.InQuad));
	    s.OnComplete(() => RandomPosition(s));
	}

    public void RandomPosition(Sequence s)
    {
	    transform.localPosition = new Vector3(-900f, Random.Range(900f, -900f), 0f);
        s.Restart();
    }

    // Update is called once per frame
    void Update () {
	
	}
}
