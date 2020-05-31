using UnityEngine;

public class Sleep : GoToObject
{
    [Header("Sleep")]
    [SerializeField] public Bed bed = null;
    bool sleeping = false;
    float zzzCounter = 2f;

    public new void OnEnable()
    {
        if(bed != null)
        {
            objectReference = bed.transform.Find("Entry").gameObject;
        }
        base.OnEnable();
    }

    public new void Start()
    {
        base.Start();
        tasks.Add("Sleep");
    }

    public new void Update()
    {
        base.Update();

        if(sleeping)
        {
            if(zzzCounter > 0f)
            {
                zzzCounter -= Time.deltaTime;
            }
            else
            {
                ai.textSpawner.SpawnText("Zzz..", true, Color.yellow);
                zzzCounter = 4f;
            }
        }
    }

    public override void TaskDone(string task = null)
    {
        if (task == "GoToObject")
        {
            sleeping = true;
            bed.Interact(ai);
            tasks.Remove("Sleep");
        }
        //base.TaskDone();
    }
}
