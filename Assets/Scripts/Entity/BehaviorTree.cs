using System;
using System.Collections.Generic;

namespace PLibrary
{
    /// <summary> 행동 트리 노드의 평가 결과 </summary>
    public enum EBTState
    {
        FAILURE, // 실패
        SUCCESS, // 성공
        RUN      // 계속 실행 (진행 일시 중지)
    }

    /// <summary> 행동 트리 </summary>
    public class BehaviorTree
    {
        private BTNode rootNode;

        /// <summary> 평가 실행, Update 계열에서 실행할 것. </summary>
        public EBTState Run()
        {
            if (rootNode == null)
                return EBTState.FAILURE;

            return rootNode.Evaluate();
        }

        public void SetRootNode(BTNode root)
        {
            rootNode = root;
        }
    }

    /// <summary> BT 노드 기반 </summary>
    public abstract class BTNode
    {
        public abstract EBTState Evaluate();
    }

    /// <summary> 자식 노드를 순서대로 실행, 하나라도 성공하면 즉시 SUCCESS. </summary>
    public class BTSelectorNode: BTNode
    {
        private List<BTNode> childNodes;


        public BTSelectorNode()
        {
            childNodes = new();
        }

        public void AddChild(BTNode node)
        {
            childNodes.Add(node);
        }

        public override EBTState Evaluate()
        {
            foreach (BTNode n in childNodes)
            {
                EBTState result = n.Evaluate();
                if (result != EBTState.FAILURE)
                    return result;
            }
            return EBTState.FAILURE;
        }
    }

    /// <summary> 자식 노드를 순서대로 실행, 모두 성공해야 SUCCESS. </summary>
    public class BTSequenceNode: BTNode
    {
        private List<BTNode> childNodes;


        public BTSequenceNode()
        {
            childNodes = new();
        }

        public void AddChild(BTNode node)
        {
            childNodes.Add(node);
        }

        public override EBTState Evaluate()
        {
            foreach (BTNode n in childNodes)
            {
                EBTState result = n.Evaluate();
                if (result != EBTState.SUCCESS)
                    return result;
            }
            return EBTState.FAILURE;
        }
    }

    /// <summary> 배정된 액션을 실행. 단말 노드 </summary>
    public class BTActionNode: BTNode
    {
        private Func<EBTState> action;

        public BTActionNode(Func<EBTState> action)
        {
            this.action = action;
        }

        public override EBTState Evaluate()
        {
            if (action == null)
                return EBTState.FAILURE;

            return action();
        }
    }
}