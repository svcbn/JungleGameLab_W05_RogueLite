using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // public bool mapON;
    private Transform _target;
    public Transform Target
    {
        get => _target;
        set
        {
            _target = value;
            if (_target != null)
            {
                // targetOffset = transform.position - _target.position;
                targetOffset = new Vector3(0,0,-10);
            }
        }
    }

    Vector3 targetOffset;
    
    public void setTarget(GameObject target){
        Target = target.transform;
    }
    private void Update() {
        if (Target != null)
        {
            transform.position = Vector3.Lerp(transform.position, Target.position + targetOffset, 0.1f);
        }
        
    }
    
    // public void ActiveMap()
    // {
    //     mapON = true;
    // }
    
    // public void InactiveMap()
    // {
    //     mapON = false;
    // }
}