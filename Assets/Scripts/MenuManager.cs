using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public GameObject mainContainer;

    public GameObject customContainer;

    public enum Container
    {
        Main = 0,
        Custom = 1
    }

    public void ChangeContainer(int container)
    {
        Container selection = (Container)container;
        mainContainer.SetActive(selection == Container.Main);
        customContainer.SetActive(selection == Container.Custom);
    }

    public void ChangeContainer(Container container)
    {
        Container selection = container;
        mainContainer.SetActive(selection == Container.Main);
        customContainer.SetActive(selection == Container.Custom);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
