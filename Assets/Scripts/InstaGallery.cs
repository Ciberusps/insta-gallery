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
    public string hashTag;
    private int _lastImageCreationTime;

    public GameObject texture;
    public UILabel hashTagLabel;
    public GameObject grid;
    
    private bool _firstRun;
    public bool downloadImages;
    public List<GameObject> allPhotoes;

    //Access_Token 2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7
    // Use this for initialization
    void Start ()
    {
        _firstRun = true;

        if (downloadImages)
        {
            RequestAllImages();
        }

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
                    var dataItem = (Dictionary<string, object>) data[i];
                    
                    var images = (Dictionary<string, object>) dataItem["images"];

                    var imagesItem = (Dictionary<string, object>) images["standard_resolution"];
                    
                    var imageLink = imagesItem["url"].ToString();

                    var caption = (Dictionary<string, object>)dataItem["caption"];

                    var creationTime = int.Parse(caption["created_time"].ToString());

                    if (creationTime > _lastImageCreationTime || _lastImageCreationTime == 0 || _firstRun)
                    {
                        GrabImage(imageLink);
                        print(imageLink);
                    }

                    if (i == 0)
                    {
                        _lastImageCreationTime = creationTime;
                        print(_lastImageCreationTime);  
                    }

                }
            }

            _firstRun = false;
        }
        else
            Debug.Log("Null response");
    }

    public void RequestAllImages()
    {
        HTTPRequest request = new HTTPRequest(new Uri("https://api.instagram.com/v1/tags/" + hashTag + "/media/recent?access_token=2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7"), OnRequestFinished);
        request.Send();
    }

    public void GrabImage(string imageLink)
    {
        GameObject obj = PhotosPoolerScript.current.GetPooledObject();

        allPhotoes.Add(obj);

        obj.transform.SetParent(UIRoot.list[0].transform);

        obj.name = "Photo_" + allPhotoes.Count;
        HTTPRequest getImageRequest =
            new HTTPRequest(new Uri(imageLink), (request, response) =>/* texture.GetComponent<UITexture>().mainTexture*/ obj.GetComponent<UITexture>().mainTexture = response.DataAsTexture2D)
                .Send();

        NGUITools.SetActive(obj, true);

        obj.transform.localScale = Vector3.one;

        obj.GetComponent<UITexture>().height = 160;
        obj.GetComponent<UITexture>().width = 160;

        obj.transform.localPosition = new Vector3(UnityEngine.Random.Range(-450f, 450f),
            UnityEngine.Random.Range(-150f, 150f), 0f);

        obj.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

        BringToFront(obj);
    }


    public void BringToFront(GameObject obj)
    {
        var objDepth = obj.GetComponent<UIWidget>().depth;

        if (allPhotoes.Count == 1)
        {
            objDepth = 11;
        }
        else
        {
            objDepth = allPhotoes[allPhotoes.Count - 2].GetComponent<UIWidget>().depth + 3;
        }

        for (int i = 0; i < obj.GetComponentsInChildren<UIWidget>().Length; i++)
        {
                obj.GetComponentsInChildren<UIWidget>()[i].depth = objDepth - 1 - i;
        }

        Debug.Log(obj.name);
    }

    public void ChangeHashTag()
    {
        hashTag = hashTagLabel.text;
    }

    public void GridView()
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
    }
}

