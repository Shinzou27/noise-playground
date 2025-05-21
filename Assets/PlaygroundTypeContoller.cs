using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public abstract class InputController
{
  
}
[Serializable]
public class SliderController : InputController
{
  public Slider input;
  public int value;
  public string key;
  public TextMeshProUGUI label;
}
[Serializable]
public class TextFieldController : InputController
{
  public TMP_InputField input;
  public string value;
  public string key;
}
[Serializable]
public class ToggleController : InputController
{
  public Toggle input;
  public bool value;
  public string key;
}
public class PlaygroundTypeContoller : MonoBehaviour
{
  public List<ToggleController> toggles;
  public List<TextFieldController> textFields;
  public List<SliderController> sliderControllers;
  public Button start;
  public PlaygroundScenes sceneToGo;
  private Animator animator;
  public Button backButton;
  void Start()
  {
    if (backButton != null)
    {
      backButton.onClick.AddListener(ReturnToMenu);
    }
    animator = GetComponent<Animator>();
    toggles.ForEach(controller => controller.input.onValueChanged.AddListener(arg => ToggleListener(arg, controller)));
    textFields.ForEach(controller => controller.input.onValueChanged.AddListener(arg => TextFieldListener(arg, controller)));
    sliderControllers.ForEach(controller =>
    {
      controller.input.onValueChanged.AddListener(arg => SliderListener(arg, controller));
      controller.label.text = controller.input.value.ToString();
    });
  }
  public void GoToScene()
  {
    toggles.ForEach(controller => PlaygroundManager.booleanVariables.Add(controller.key, controller.value));
    textFields.ForEach(controller => PlaygroundManager.stringVariables.Add(controller.key, controller.value));
    sliderControllers.ForEach(controller => PlaygroundManager.integerVariables.Add(controller.key, controller.value));
    SceneManager.LoadSceneAsync(sceneToGo.ToString());
  }
  public void UpdateVariables()
  {
    PlaygroundManager.booleanVariables.Clear();
    PlaygroundManager.stringVariables.Clear();
    PlaygroundManager.integerVariables.Clear();
    toggles.ForEach(controller => PlaygroundManager.booleanVariables.Add(controller.key, controller.value));
    textFields.ForEach(controller => PlaygroundManager.stringVariables.Add(controller.key, controller.value));
    sliderControllers.ForEach(controller => PlaygroundManager.integerVariables.Add(controller.key, controller.value));
    PlaygroundManager.OnVariableChange?.Invoke(this, EventArgs.Empty);
  }

  private void SliderListener(float arg0, InputController controller)
  {
    if (((SliderController)controller).key == "CAVE_GEN_THRESHOLD")
    {
      ((SliderController)controller).value = (int)(arg0 * 100);
      ((SliderController)controller).label.text = string.Format("{0:0.##}", arg0);
    }
    else
    {
      ((SliderController)controller).value = (int)arg0;
      ((SliderController)controller).label.text = ((int)arg0).ToString();
    }
  }

  private void TextFieldListener(string arg0, InputController controller)
  {
    ((TextFieldController)controller).value = arg0;
  }

  private void ToggleListener(bool arg0, InputController controller)
  {
    ((ToggleController)controller).value = arg0;
  }
  public void Open()
  {
    animator.Play("In");
  }
  public void Close()
  {
    animator.Play("Out");
  }
  public void ReturnToMenu()
  {
    SceneManager.LoadSceneAsync(PlaygroundScenes.MenuScene.ToString());
    PlaygroundManager.integerVariables.Clear();
    PlaygroundManager.stringVariables.Clear();
    PlaygroundManager.booleanVariables.Clear();
  }
}
