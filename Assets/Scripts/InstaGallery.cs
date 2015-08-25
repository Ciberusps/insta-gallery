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
    //Access_Token 2029806223.de28c8b.1ae64c12db4d45dcb28139d6b684ada7
    // Use this for initialization
    void Start ()
    {
        _firstRun = true;
        RequestAllImages();
      
    }

    public string hashTag;
    public GameObject texture;
    private int _lastImageCreationTime;
    private bool _firstRun;
    public List<GameObject> allPhotoes; 

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

        if (allPhotoes.Count == 0)
            obj.GetComponent<UIWidget>().depth = 10;
        else
        {
            BringToFront(obj);
        }

        allPhotoes.Add(obj);
    }


    public void BringToFront(GameObject obj)
    {
        obj.GetComponent<UIWidget>().depth = allPhotoes[allPhotoes.Count - 1].GetComponent<UIWidget>().depth + 1;

        for (int i = 0; i < obj.GetComponentsInChildren<UIWidget>().Length; i++)
        {
            obj.GetComponentsInChildren<UIWidget>()[i].depth = obj.GetComponent<UIWidget>().depth + 1;
        }
    }
}

