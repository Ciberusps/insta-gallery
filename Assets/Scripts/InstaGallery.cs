using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.JSON;

/*
[ExecuteInEditMode]
*/
public class InstaGallery : MonoBehaviour
{
    public static InstaGallery current;
    public GameObject texture;
    public UILabel hashTagLabel;
    public GameObject grid;

    //public List<GameObject> allPhotoes;
    public List<GameObject> Photoes; 
    public string hashTag;
    public bool downloadImages;
    public int countPhotoes;

    private int _lastImageCreationTime;
    private bool _firstRun;
    private int _maxDepth;
    private string maxId, nextUrl;
    private GameObject photoGO;

    //Access_Token 2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7
    // Use this for initialization
    void Start ()
    {
        _firstRun = true;

        if (downloadImages)
        {
            RequestAllImages();
        }

        current = this;

        _maxDepth = 11;
    }

    public void Update()
    {
                
    }

    void OnRequestFinished(HTTPRequest request, HTTPResponse response)
    {

        if (response != null)
        {
            //response.Data.GetLength();
            Debug.Log("Request Finished, Text received: " + response.DataAsText);
            //var root =(Hashtable) MiniJSON.JsonDecode("{\"data\":[]}");
            var root = (Dictionary<string,object>)Json.Decode(response.DataAsText);
            
            if (root.ContainsKey("data"))
            {
                var data = (List<object>)root["data"];
                
                for (var i = 0; i < data.Count; i++)
                {

                    photoGO = PhotosPoolerScript.current.GetPooledObject();

                    Photo myPhoto = photoGO.GetComponent<Photo>();

                    Photoes.Add(photoGO);

                    var dataItem = (Dictionary<string, object>) data[i];
                    var images = (Dictionary<string, object>) dataItem["images"];
                    var imagesItem = (Dictionary<string, object>) images["standard_resolution"];
                    var caption = (Dictionary<string, object>)dataItem["caption"];

                    myPhoto.link = imagesItem["url"].ToString();
                    Debug.Log(myPhoto.link);
                    //var imageLink = imagesItem["url"].ToString();
                    myPhoto.createdTime = int.Parse(caption["created_time"].ToString());
                    //var creationTime = int.Parse(caption["created_time"].ToString());

              
                    var user12345 = (Dictionary<string, object>)dataItem["user"];
                    myPhoto.user.name = user12345["username"].ToString();
                    myPhoto.user.profilePicture = user12345["profile_picture"].ToString();
                    myPhoto.user.id = user12345["id"].ToString();
                    myPhoto.user.profilePicture = user12345["full_name"].ToString();


                    /*Debug.Log(myPhoto.link);
                    Debug.Log(myPhoto.createdTime);
                    Debug.Log(myPhoto.user.name);
                    Debug.Log(myPhoto.user.id);
                    Debug.Log(myPhoto.user.id);
                    Debug.Log(myPhoto.user.profilePicture);*/

                    GrabImage(photoGO, myPhoto);

                    /*if (photo.createdTime > _lastImageCreationTime || _lastImageCreationTime == 0 || _firstRun)
                    {
                        //GrabImage(imageLink);
                        //print(imageLink);
                    }

                    if (i == 0)
                    {
                        _lastImageCreationTime = photo.createdTime;
                        //print(_lastImageCreationTime);  
                    }*/

                }

                if (root.ContainsKey("pagination"))
                {
                    var pagination = (Dictionary<string, object>)root["pagination"];

                    //maxId = pagination["next_max_id"].ToString();
                    if (pagination.ContainsKey("next_url"))
                        nextUrl = pagination["next_url"].ToString();
                    else
                        nextUrl = null;
                    Debug.Log(maxId);

                    /*string[] s = maxId.Split(new char[] { '_' });
                    maxId = s[0];
                    Debug.Log(maxId);*/
                }

                if (data.Count == countPhotoes && nextUrl != null)
                {
                    //Invoke("RequestRecentImages", 3f);
                    RequestRecentImages();
                }

            }

            

            _firstRun = false;
        }
        else
            Debug.Log("Null response");
    }

    public void RequestAllImages()
    {
        HTTPRequest request = new HTTPRequest(new Uri("https://api.instagram.com/v1/tags/" + hashTag + "/media/recent?access_token=2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7&count=" + countPhotoes), OnRequestFinished);
        request.Send();
        Debug.Log("One update");
    }

    public void RequestRecentImages()
    {
        //HTTPRequest request = new HTTPRequest(new Uri("https://api.instagram.com/v1/tags/" + hashTag + "/media/recent?access_token=2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7&count=" + countPhotoes + "&max_id=" + maxId), OnRequestFinished);
        Debug.Log(nextUrl);
        HTTPRequest request = new HTTPRequest(new Uri(nextUrl), OnRequestFinished);
        request.Send();
        Debug.Log("Two update");

    }

    public void GrabImage(GameObject photoGO, Photo photo)
    {
        photoGO.transform.SetParent(UIRoot.list[0].transform);

        photoGO.name = "Photo_" + Photoes.Count;
        HTTPRequest getImageRequest =
            new HTTPRequest(new Uri(photo.link), (request, response) => photoGO.GetComponent<UITexture>().mainTexture = response.DataAsTexture2D)
                .Send();

        NGUITools.SetActive(photoGO.gameObject, true);

        photoGO.transform.localScale = Vector3.one;

        photoGO.GetComponent<UITexture>().height = 160;
        photoGO.GetComponent<UITexture>().width = 160;

        RandomizePhotoPosition(photoGO);

        BringToFront(photoGO);
    }

    /*public void GrabImage(string imageLink)
    {
        GameObject obj = PhotosPoolerScript.current.GetPooledObject();

        allPhotoes.Add(obj);

        obj.transform.SetParent(UIRoot.list[0].transform);

        obj.name = "Photo_" + allPhotoes.Count;
        HTTPRequest getImageRequest =
            new HTTPRequest(new Uri(imageLink), (request, response) =>/* texture.GetComponent<UITexture>().mainTexture#1# obj.GetComponent<UITexture>().mainTexture = response.DataAsTexture2D)
                .Send();

        NGUITools.SetActive(obj, true);

        obj.transform.localScale = Vector3.one;

        obj.GetComponent<UITexture>().height = 160;
        obj.GetComponent<UITexture>().width = 160;

        RandomizePhotoPosition(obj);

        BringToFront(obj);
    }*/



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


       //Debug.Log(obj.name + " " + objDepth);
    }

    public void ChangeHashTag()
    {
        hashTag = hashTagLabel.text;
    }

    /*public void GridView()
    {
        for (var i = 0; i < allPhotoes.Count; i++)
        {
            allPhotoes[i].transform.SetParent(grid.transform);
        }

        grid.GetComponent<UIGrid>().enabled = true;
    }

    public void RandomView()
    {
        for (var i = 0; i < allPhotoes.Count; i++)
        {
            allPhotoes[i].transform.SetParent(UIRoot.list[0].transform);
        }
    }*/

    public void RandomizePhotoPosition(GameObject obj)
    {
        obj.transform.localPosition = new Vector3(UnityEngine.Random.Range(-550f, 550f),
            UnityEngine.Random.Range(-150f, 150f), 0f);

        obj.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
    }
}

