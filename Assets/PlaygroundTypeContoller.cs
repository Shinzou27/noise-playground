using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
  public Button goBack;
  public List<ToggleController> toggles;
  public List<TextFieldController> textFields;
  public List<SliderController> sliderControllers;
  public Button start;
  void Start()
  {
    toggles.ForEach(controller => controller.input.onValueChanged.AddListener(arg => ToggleListener(arg, controller)));
    textFields.ForEach(controller => controller.input.onValueChanged.AddListener(arg => TextFieldListener(arg, controller)));
    sliderControllers.ForEach(controller => controller.input.onValueChanged.AddListener(arg => SliderListener(arg, controller)));
  }

  private void SliderListener(float arg0, InputController controller)
  {
    ((SliderController)controller).value = (int)arg0;
  }

  private void TextFieldListener(string arg0, InputController controller)
  {
    ((TextFieldController)controller).value = arg0;
  }

  private void ToggleListener(bool arg0, InputController controller)
  {
    ((ToggleController)controller).value = arg0;
  }
}
