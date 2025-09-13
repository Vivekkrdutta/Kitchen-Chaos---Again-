using UnityEngine;

public class LoaderCallBack : MonoBehaviour
{
    bool firstTime = true;
    private void Awake()
    {
        firstTime = true;
    }
    private void Update()
    {
        
        if (firstTime )
        {
            firstTime = false;
            Loader.LoaderCallBack();
        }
    }
}
