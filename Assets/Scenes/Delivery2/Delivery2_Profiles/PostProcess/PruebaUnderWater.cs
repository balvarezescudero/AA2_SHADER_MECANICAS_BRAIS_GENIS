using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
[ExecuteInEditMode]

public class PruebaUnderWater : MonoBehaviour
{
    // Start is called before the first frame update
    public PostProcessVolume post;
    CustomPostproPixelateSettings pix;
    CustomPostproVignetteSettings vig;
    bool reset = true;

    float pixAmount = 0.001f;

    void Start()
    {
        post.profile.TryGetSettings(out pix);
        post.profile.TryGetSettings(out vig);
        pix.blend.overrideState = true;
        vig.blend.overrideState = true;
        pix.blend.value = 0.01f;
        vig.blend.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
       if(Camera.main.transform.position.y < 2.5f)
        {
           
            pix.blend.value = Mathf.Lerp(pix.blend, 8f, Time.deltaTime);
            vig.blend.value = Mathf.Lerp(vig.blend, 10f, Time.deltaTime/2);

        }
        else if(pix.blend.value > 0.03f)
        {
            pix.blend.value = Mathf.Lerp(pix.blend, 0.01f, Time.deltaTime);
            vig.blend.value = Mathf.Lerp(vig.blend, 0f, Time.deltaTime);
        }


    }
 
}
