using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundPerlinController : MonoBehaviour
{
    public enum FlowState {
        MENU,
        STEP_BY_STEP_2D,
        SURFACE_GENERATOR,
        GENERATOR_3D,
    };
    public FlowState current = FlowState.MENU;
    [SerializeField] private GameObject menuContainer;
    [SerializeField] private GameObject stepByStep2DContainer;
    [SerializeField] private GameObject surfaceContainer;
    [SerializeField] private GameObject generator3DContainer;
    public void ChangeToMenu() {
        current = FlowState.MENU;
    }
    public void ChangeTo2DStepByStep() {
        current = FlowState.STEP_BY_STEP_2D;
    }
    public void ChangeToSurfaceGen() {
        current = FlowState.SURFACE_GENERATOR;
    }
    public void Change3DGen() {
        current = FlowState.GENERATOR_3D;
    }
    void Update()
    {
        menuContainer.SetActive(current == FlowState.MENU);
        stepByStep2DContainer.SetActive(current == FlowState.STEP_BY_STEP_2D);
        surfaceContainer.SetActive(current == FlowState.SURFACE_GENERATOR);
        generator3DContainer.SetActive(current == FlowState.GENERATOR_3D);
        
    }
}
