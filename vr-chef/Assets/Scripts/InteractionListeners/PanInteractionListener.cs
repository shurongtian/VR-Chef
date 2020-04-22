﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class PanInteractionListener : InteractionListener
{
    public static Color DefaultOutlineColor = Color.white;
    public static Color SelectedOutlineColor = new Color(255.0f / 255.0f, 228.0f / 255.0f, 0.0f / 255.0f);
    public static Color ChoppedOutlineColor = new Color(0.0f / 255.0f, 0.0f / 255.0f, 0.0f / 255.0f, 0.5f);
    public static float SelectedOutlineMultiplier = 2.0f;
    public GameObject dialObject;

    Rigidbody rb;
    Renderer renderer;
    bool isGrabbed = false;
    float defaultOutlineWidth;
    Vector3 originalRotationOnGrab;
    int chopCount = 0;
    GameObject ingredient;

    void UpdateMaterial(bool isNearHand)
    {
        foreach (var material in renderer.materials)
        {
            if (chopCount > 0)
            {
                material.SetColor("_Tint", ChoppedOutlineColor);
                material.SetColor("_OutlineColor", ChoppedOutlineColor);
                material.SetFloat("_OutlineWidth", isNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
            }
            else
            {
                material.SetColor("_Tint", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
                material.SetColor("_OutlineColor", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
                material.SetFloat("_OutlineWidth", isNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
            }
        }

        renderer.UpdateGIMaterials();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RecipeIngredient"))
        {
            print("Collision detected in " + this.gameObject.name);
            print("Collided with " + collision.gameObject.name);
            if (gameObject.transform.position.y < collision.transform.position.y)
            {
                print(gameObject.name + " is below " + collision.gameObject.name);
                this.ingredient = collision.gameObject;
                RecipeInteractionListener recipeInteraction = this.ingredient.GetComponent<RecipeInteractionListener>();
                recipeInteraction.changeIngredientColor(50);
            }

        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RecipeIngredient"))
        {
            if (this.ingredient == collision.gameObject)
            {
                print(this.ingredient.name + " is no longer above " + this.gameObject.name);
                this.ingredient = null;
            }
        }
    }

    public void Start()
    {
        renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();
        defaultOutlineWidth = renderer.material.GetFloat("_OutlineWidth");

        //update material
        UpdateMaterial(false);
    }

    public override void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
        if (isGrabbed)
        {
            var hand = controller.GetHand();
            var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();
            gameObject.transform.position = controller.Target.transform.position;

            Vector3 newAngles = controller.Target.transform.rotation.eulerAngles - originalRotationOnGrab + controller.RotationBias;
            gameObject.transform.rotation = Quaternion.Euler(newAngles);
            handRenderer.enabled = false;
        }
    }

    public override void OnEnterClosest(InteractionController controller)
    {
        //don't do anything if there's an object in the hand
        if (controller.ControlledObject) return;

        UpdateMaterial(true);
    }

    public override void OnLeaveClosest(InteractionController controller)
    {
        UpdateMaterial(false);
    }

    public override void OnGrab(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        originalRotationOnGrab = controller.Target.transform.rotation.eulerAngles;

        //make the hand mesh invisible
        isGrabbed = true;
        handRenderer.enabled = false;
        rb.isKinematic = true;

        UpdateMaterial(false);
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        //remove food item
        isGrabbed = false;
        handRenderer.enabled = true;
        rb.isKinematic = false;
    }
}
