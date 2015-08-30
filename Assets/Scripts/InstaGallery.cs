using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BestHTTP;
using BestHTTP.JSON;
using DG.Tweening;
using SimpleJSON;

/*
[ExecuteInEditMode]
*/
public class InstaGallery : MonoBehaviour
{
    public static InstaGallery current;

    public int maxPhotoesCount;
    public string hashTag;
    public int countPhotoes;
    public float tweenDuration;
    public UILabel hashTagLabel;
    public Transform target;
    public Transform photoStation;
    public GameObject grid;
    public List<GameObject> Photoes;

    private int _lastRequestTime, _thisRequestTime;
    private bool _firstRun;
    private bool _firstRequest;
    private int _maxDepth;
    private string maxId, nextUrl;
    private GameObject photoGO;

    //Access_Token 2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7
    // Use this for initialization
    void Start ()
    {
        current = this;

        _firstRun = true;
        _firstRequest = true;
        _maxDepth = 11;

        nextUrl = "https://api.instagram.com/v1/tags/" + hashTag +
                  "/media/recent?access_token=2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7&count=" +
                  countPhotoes;
    }

    public void TakePhotoesRequest()
    {
        print(nextUrl);
        StartCoroutine("WWWRequest");
    }

    public IEnumerator WWWRequest()
    {
        WWW request =
            new WWW(nextUrl);
        yield return request;
        
        var root =  JSON.Parse(request.text);
  
        var data = root["data"];

        for (var i = 0; i < data.Count; i++)
        {
            photoGO = PhotosPoolerScript.current.GetPooledObject();

            Photo myPhoto = photoGO.GetComponent<Photo>();

            //Photoes.Add(photoGO);

            var dataItem = data[i];
            myPhoto.link = dataItem["images"]["standard_resolution"]["url"];
            myPhoto.createdTime = int.Parse(dataItem["caption"]["created_time"]);

            if (_firstRequest && _firstRun && i == 0)
                _lastRequestTime = myPhoto.createdTime;
            if (_firstRequest && i == 0)
                _thisRequestTime = myPhoto.createdTime;


            var user = dataItem["user"];
            myPhoto.user.name = user["username"];
            myPhoto.user.profilePicture = user["profile_picture"];
            myPhoto.user.id = user["id"];
            myPhoto.user.profilePicture = user["full_name"];

            print(myPhoto.createdTime + " < " + _lastRequestTime);
            
            if (!_firstRun && myPhoto.createdTime <= _lastRequestTime)
            { 
                NGUITools.SetActive(photoGO, false);
                myPhoto.ResetPhoto();
            }
            else if (Photoes.Count  < maxPhotoesCount)
            {
                StartCoroutine(GrabNewImage(photoGO, myPhoto));
                Photoes.Add(photoGO);
            }
        }

        var pagination = root["pagination"];
        nextUrl = pagination["next_url"];

        if (data.Count == countPhotoes && nextUrl != null)
        {
            _firstRequest = false;
            if (Photoes.Count < maxPhotoesCount)
                StartCoroutine("WWWRequest");
        }
        else
        {
            nextUrl = "https://api.instagram.com/v1/tags/" + hashTag +
                      "/media/recent?access_token=2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7&count=" +
                      countPhotoes;

            if (!_firstRun)
            {
                if (_thisRequestTime > _lastRequestTime)
                    _lastRequestTime = _thisRequestTime;
            }

            _firstRequest = true;
            _firstRun = false;
        }

    }

    IEnumerator GrabNewImage(GameObject photoGO, Photo photo)
    {
        //BringToGrid(photoGO);
        //photoGO.transform.SetParent(target);
        //photoGO.transform.localScale = Vector3.one;

        photoGO.name = "Photo_" + Photoes.Count;

        WWW getImageRequest = new WWW(photo.link);    
        NGUITools.SetActive(photoGO.gameObject, true);
        yield return getImageRequest;

        //print(getImageRequest);

        photoGO.GetComponent<UITexture>().mainTexture = getImageRequest.texture;


        /*photoGO.GetComponent<UITexture>().height = 160;
        photoGO.GetComponent<UITexture>().width = 160;*/
        BringToPhotoStation(photoGO);
        BringToFront(photoGO);

        RandomizePhotoPosition(photoGO);
        //target.GetComponent<UIGrid>().enabled = true;

    }

    public void BringToFront(GameObject obj)
    {
        var objDepth = obj.GetComponent<UIWidget>().depth;

        if (Photoes.Count == 1)
        {
            objDepth = _maxDepth;
        }
        else
        {
            _maxDepth += 3;
            objDepth = _maxDepth;
        }

        for (int i = 0; i < obj.GetComponentsInChildren<UIWidget>().Length; i++)
        {
            obj.GetComponentsInChildren<UIWidget>()[i].depth = objDepth - 1 - i;
        }
    }

    public void ChangeHashTag()
    {
        if (hashTag != hashTagLabel.text)
        {
            hashTag = hashTagLabel.text;
            _firstRun = true;
            _thisRequestTime = 0;
            _lastRequestTime = 0;

            nextUrl = "https://api.instagram.com/v1/tags/" + hashTag +
                      "/media/recent?access_token=2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7&count=" +
                      countPhotoes;
        }
        ClearPhotoes();

    }

    public void ClearPhotoes()
    {
        foreach (GameObject GO in Photoes)
        {
            GO.GetComponent<Photo>().ResetPhoto();
            NGUITools.SetActive(GO, false);
        }
        Photoes.Clear();
    }

    public void RandomizePhotoPosition(GameObject obj)
    {
        obj.transform.localPosition = new Vector3(-900f, UnityEngine.Random.Range(900f, -900f), 0f);

        Sequence s = DOTween.Sequence();

        s.Append(
            obj.transform.DOLocalMove(new Vector3(UnityEngine.Random.Range(-550f, 550f), UnityEngine.Random.Range(-250f, 200f), 0f), tweenDuration)
                .SetEase(Ease.OutBack));
        s.Insert(0,
            obj.transform.DORotate(new Vector3(UnityEngine.Random.Range(5, 15), 0, UnityEngine.Random.Range(5, 15)), tweenDuration / 2).SetEase(Ease.InQuad).SetLoops(1, LoopType.Yoyo));
        s.Insert(0,
            obj.transform.DORotate(new Vector3(obj.transform.localRotation.x, 0, UnityEngine.Random.Range(5, 15)), tweenDuration / 2).SetEase(Ease.InQuad).SetLoops(1, LoopType.Yoyo));
        //s.Insert(2,
        //  transform.DORotate(new Vector3(0, 0, 0), duration/2).SetEase(Ease.InQuad));
    }

    public void BringToPhotoStation(GameObject photoGO)
    {
        photoGO.transform.SetParent(photoStation);
    }

    public void BringToGrid(GameObject photoGO)
    {
        photoGO.transform.SetParent(grid.transform);
    }

    public void SortByTime()
    {
        
    }
}

