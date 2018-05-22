using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************************************************************
* Write your AI code in this file here. The private variable 'agentScript' contains all the agents actions which are listed
* below. Ensure your code it clear and organised and commented.
* 
* 'agentScript' properties
* -  public bool Alive                                  // Check if we are still alive
* -  public bool PowerUp                                // Check if we have used the power up
* -  public int CurrentHitPoints                        // How many current hit points do we have
*
* 'agentScript' methods    
* -  public void MoveTo(GameObject target)               // Move towards a target object        
* -  public void RandomWander()                          // Randomly wander around the level    
* -  public bool IsInAttackRange(GameObject enemy)       // Check if we're with attacking range of the enemy    
* -  public void AttackEnemy(GameObject enemy)           // Attack the enemy
* -  public void Flee(GameObject enemy)                  // Run away
* -  public bool IsObjectInView(String name)             // Check if something of interest is in range
* -  public GameObject GetObjectInView(String name)      // Get a percieved object, null if object is not in view
* 
*****************************************************************************************************************************/

//The below class includes all of the actions that we want the Agent to be able to take.
//These will be set in inside nodes that will allow these actions to be executed
public class Actions
{
    //This method calls the agents random wander function
    public static void Wander(AgentActions Agent, string target)
    {
        Debug.Log("Wandering");
        Agent.RandomWander();
    }

    //This method calls the agents flee function and uses the enemy gameobject as a target
    public static void Flee(AgentActions Agent, string target)
    {
        Debug.Log("Fleeing");
        Agent.Flee(Agent.Enemy);
    }

    //This method calls the agents attack function with the enemy gameobject as a target
    public static void Attack(AgentActions Agent, string target)
    {
        Debug.Log("Attacking");
        Agent.AttackEnemy(Agent.Enemy);
    }

    //This method calls the agents move to function and the agents get object function by passing
    //a string into it.
    public static void MoveTo(AgentActions Agent, string target)
    {
        Debug.Log("Moving to target");
        Agent.MoveTo(Agent.GetObjectInView(target));
    }

}

//public delegate that will act as a pointer towards all of the methods listed above
//as we cannot directly access the instance of the AgentActions
public delegate void dActions(AgentActions agent, string target);

//The below class contains all of the decisions that the Agent will need to consider to allow for it to 
//take the correct course of action depending on the circumstances
public class Decisions
{
    //This method checks to see if the agent is below 25 health and returns a bool
    public static bool IsHealthBelow25(AgentActions Agent, string objectName)
    {
        return Agent.CurrentHitPoints < 25;     
    }

    //This method checks to see if the enemy is below 25 health and returns a bool
    public static bool IsEnemyHealthBelow25(AgentActions Agent, string objectName)
    {
        return Agent.Enemy.GetComponent<AgentActions>().CurrentHitPoints < 25;
    }

    //This method checks to see if an object is in sight for the agent using a string that is passed into the function
    public static bool IsObjectInSight(AgentActions Agent, string ObjectName)
    {
        return Agent.IsObjectInView(ObjectName);
    }

    //This method checks to see if the agent is in range of the enemy
    public static bool InRangeOfEnemy(AgentActions Agent, string objectName)
    {
        return Agent.IsInAttackRange(Agent.Enemy);
    }

    //This method checks to see if the agent is powered up and returns a bool
    public static bool PoweredUp(AgentActions Agent, string objectName)
    {
        return Agent.HasPowerUp;
    }

    //This method checks to see if the enemy is powered up and returns a bool
    public static bool EnemyPoweredUp(AgentActions Agent, string objectName)
    {
        return Agent.Enemy.GetComponent<AgentActions>().HasPowerUp;
    }
}

//public delegate that will act as a pointer towards all of the methods listed above
//as we cannot directly access the instance of the AgentActions and pass a method.
public delegate bool dDecisions(AgentActions agent, string objectName);

//A base class of a node that will be used to build the Decision Node and the Action Node
//
public abstract class Node
{
    //These functions are defined here to stop compiler errors
    public abstract void RunAction();
    public abstract Node MakeDecision();

    //bool that checks to see if the current node is an action or not
    public bool IsAction;

    
}

//Decision class child from a node that takes a decision and stores it
public class DecisionNode : Node
{
    //Members of the node
    dDecisions mDecision;
    AgentActions mAgent;
    string mName;
    
    //nodes children that make up the tree
    public Node mNodeTrue;
    public Node mNodeFalse;

    //custom constructor that takes a delegate, a copy of the agent actions and a name
    public DecisionNode(dDecisions Decision, AgentActions Agent, string Name)
    {
        IsAction = false;
        
        mAgent = Agent;
        mDecision = Decision;
        mName = Name;
    }

    //Adds a true node child to the node
    public void AddNodeTrue(Node child)
    {
        mNodeTrue = child;
       
    }

    //Adds a false node child to the node
    public void AddNodeFalse(Node child)
    {
        mNodeFalse = child;
    }

    //Method that checks the delegate stored in the class and returns a true 
    //or false node depending on the result
    public override Node MakeDecision()
    {
        if (mDecision.Invoke(mAgent, mName) == true)
        {
            return mNodeTrue;
        }
        else
        {
            return mNodeFalse;
        }
    }

    //declared to avoid compiler errors
    public override void RunAction() {}
}

//Child class of the node that stores an action to be executed
public class ActionNode : Node
{
    //Class members
    dActions mNodeAction;
    string mTarget;
    AgentActions mAgent;

   
    //Custom constructor that takes a delegate, an agent action script and a string
    public ActionNode(dActions Action, AgentActions Agent, string Target)
    {
        IsAction = true;

        mAgent = Agent;
        mNodeAction = Action;
        mTarget = Target;
    }

    //This method is not required in an action node so we return null to avoid any unwanted behaviour
    public override Node MakeDecision()
    {
        return null;
    }

    //Runs the delegate stored in the class;
    public override void RunAction()
    {
        mNodeAction.Invoke(mAgent,mTarget);
    }
}

//Decision tree class for the AI
public class DecisionTree : Node
{
    //Members for the root node and the current node
    public Node mRootNode;
    public Node mCurrentNode;

    public DecisionTree(Node rootnode)
    {
        mRootNode = rootnode;
    }

    public override Node MakeDecision()
    {
        return mRootNode.MakeDecision();
    }

    //public function that runs this AI's tree
    public bool ExecuteTree()
    {
        RunThroughTree(mRootNode);
        return true;
    }

    //Recursive function that goes through all of the nodes and runs an action when it finds an action node
    protected void RunThroughTree(Node CurrentNode)
    {
        if(CurrentNode.IsAction == true)
        {
            CurrentNode.RunAction();
        }
        else
        {
           mCurrentNode = CurrentNode.MakeDecision();
           RunThroughTree(mCurrentNode);
           
        }
    }

    public override void RunAction()
    {
        //Decision Nodes do not need to run actions so no code is required here
    }
}


public class AI : MonoBehaviour
{
    // This is the script containing the AI agents actions
    // e.g. agentScript.MoveTo(enemy);
    private AgentActions agentScript;

    //Enemy's gameobject name
    private string mEnemyName;

    //The instance of the decision tree
    private DecisionTree AITree;



    private void Awake()
    {
        agentScript = this.gameObject.GetComponent<AgentActions>();

        //Checks the name of the Object so it can create a string to pass through certain decisions
        if (this.gameObject.name == "AIAgent1")
            mEnemyName = "AIAgent2";
        else
            mEnemyName = "AIAgent1";


        //write DecisionTree here
  
        //Root node for the tree that will be created
        //This tests for the health of the entity to decide the initial set of decisions
        dDecisions HealthLow = new dDecisions(Decisions.IsHealthBelow25);
        DecisionNode EntityHPCheck = new DecisionNode(HealthLow, agentScript, null);

        //Creates a tree that can go through the nodes that will be used to create the AI
        AITree = new DecisionTree(EntityHPCheck);

        //Left branch of the decision tree from the root

        //Enemy in sight node, used for true side
        dDecisions EnemyLook = new dDecisions(Decisions.IsObjectInSight);
        DecisionNode EnemyInSightCheck = new DecisionNode(EnemyLook, agentScript, mEnemyName);
        EntityHPCheck.AddNodeTrue(EnemyInSightCheck);

        //Flee from enemy action node - Will be reused for other flee actions
        dActions FleeEnemy = new dActions(Actions.Flee);
        ActionNode ActionFlee = new ActionNode(FleeEnemy, agentScript, null);
        EnemyInSightCheck.AddNodeTrue(ActionFlee);

        //Decision node that checks for any health kits in sight
        dDecisions HealthLook = new dDecisions(Decisions.IsObjectInSight);
        DecisionNode HealthInSightCheck = new DecisionNode(HealthLook, agentScript, "Health Kit");
        EnemyInSightCheck.AddNodeFalse(HealthInSightCheck);

        //Action node that moves towards health when it is in sight after the decision check
        dActions MoveToObject = new dActions(Actions.MoveTo);
        ActionNode ActionHealthPickup = new ActionNode(MoveToObject, agentScript, "Health Kit");
        HealthInSightCheck.AddNodeTrue(ActionHealthPickup);

        //Decision node that checks to see if you are powered up
        dDecisions CheckPowerUp = new dDecisions(Decisions.PoweredUp);
        DecisionNode DecisionEntityPowered = new DecisionNode(CheckPowerUp, agentScript, null);
        HealthInSightCheck.AddNodeFalse(DecisionEntityPowered);

        //Action node that wanders around
        dActions WanderAround = new dActions(Actions.Wander);
        ActionNode ActionWander = new ActionNode(WanderAround, agentScript, null);
        DecisionEntityPowered.AddNodeTrue(ActionWander);

        //Decision node that checks to see if a power up is in sight
        dDecisions PowerLook = new dDecisions(Decisions.IsObjectInSight);
        DecisionNode DecisionPowerInSight = new DecisionNode(PowerLook, agentScript, "Power Up");
        DecisionEntityPowered.AddNodeFalse(DecisionPowerInSight);

        //Action node that reuses a delegate and another action node to add children to the power in sight check
        ActionNode ActionPowerPickup = new ActionNode(MoveToObject, agentScript, "Power Up");
        DecisionPowerInSight.AddNodeTrue(ActionPowerPickup);
        DecisionPowerInSight.AddNodeFalse(ActionWander);

        //Right branch of the decision tree from the root node

        //Decision node that checks if enemy is in sight
        DecisionNode DecisionRightEnemyInSight = new DecisionNode(EnemyLook, agentScript, mEnemyName);
        EntityHPCheck.AddNodeFalse(DecisionRightEnemyInSight);

        //Adds the power check and children to the false decision
        DecisionRightEnemyInSight.AddNodeFalse(DecisionEntityPowered);

        //Decision node that checks the enemy health value
        dDecisions EnemyHealthLow = new dDecisions(Decisions.IsEnemyHealthBelow25);
        DecisionNode DecisionEnemyHPCheck = new DecisionNode(EnemyHealthLow, agentScript, null);
        DecisionRightEnemyInSight.AddNodeTrue(DecisionEnemyHPCheck);

        //Adds the wander action node to the true decision
        DecisionEnemyHPCheck.AddNodeTrue(ActionWander);

        //Decision node that checks if the enemy is powered up
        dDecisions EnemyPoweredUp = new dDecisions(Decisions.EnemyPoweredUp);
        DecisionNode DecisionEnemyPowered = new DecisionNode(EnemyPoweredUp, agentScript, null);
        DecisionEnemyHPCheck.AddNodeFalse(DecisionEnemyPowered);

        //Checks to see if the entity is powered after the enemy is not powered up
        DecisionNode DecisionEntityPoweredEnemyNot = new DecisionNode(CheckPowerUp, agentScript, null);
        DecisionEnemyPowered.AddNodeFalse(DecisionEntityPoweredEnemyNot);


        //Decision node checks to see if the entity is in range 
        dDecisions EnemyInRange = new dDecisions(Decisions.InRangeOfEnemy);
        DecisionNode DecisionEnemyInRange = new DecisionNode(EnemyInRange, agentScript, null);
        DecisionEntityPoweredEnemyNot.AddNodeFalse(DecisionEnemyInRange);
        DecisionEntityPoweredEnemyNot.AddNodeTrue(ActionWander);

        //Action node that attacks an enemy
        dActions AttackEnemy = new dActions(Actions.Attack);
        ActionNode ActionAttack = new ActionNode(AttackEnemy, agentScript, null);
        DecisionEnemyInRange.AddNodeTrue(ActionAttack);

        //Action node that moves towards the enemy
        ActionNode ActionMoveToEnemy = new ActionNode(MoveToObject, agentScript, mEnemyName);
        DecisionEnemyInRange.AddNodeFalse(ActionMoveToEnemy);

        //Decision node that checks if you are powered up
        DecisionNode DecisionRightPowered = new DecisionNode(CheckPowerUp, agentScript, null);
        DecisionEnemyPowered.AddNodeTrue(DecisionRightPowered);

        //Reused action node that flees if you are not powered up
        DecisionRightPowered.AddNodeFalse(ActionFlee);

        //reused decision node and attached children
        DecisionRightPowered.AddNodeTrue(DecisionEnemyInRange);


    }

	// Update is called once per frame
	void Update ()
    {
        //Checks to see if the agent is alive and if so executes it's AI
        if (agentScript.Alive)
        {
            AITree.ExecuteTree();
        }        
    }
}
