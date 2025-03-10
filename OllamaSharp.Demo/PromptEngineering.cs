using System;
using System.Collections.Generic;
using System.Text;

namespace OllamaSharp.Demo
{
    /// <summary>
    /// 提示词工程类 - 用于构建高质量的提示词
    /// </summary>
    public class PromptEngineering
    {
        /// <summary>
        /// 几种常见的提示词策略
        /// </summary>
        public enum PromptStrategy
        {
            // 基本策略
            Direct,          // 直接提问
            CoT,             // 思维链 (Chain-of-Thought)
            ZeroShot,        // 零样本学习
            FewShot,         // 少样本学习
            
            // 高级策略
            ReAct,           // 思考-行动-观察循环
            TreeOfThoughts,  // 思维树
            SelfConsistency, // 自洽性检查
            RAG              // 检索增强生成
        }
        
        /// <summary>
        /// 应用思维链（Chain-of-Thought）提示策略
        /// </summary>
        public static string ApplyCoT(string basePrompt)
        {
            return $@"{basePrompt}

请一步一步思考这个问题，先分析所有已知条件，然后逐步推理，最后给出结论。";
        }
        
        /// <summary>
        /// 应用少样本（Few-Shot）提示策略
        /// </summary>
        public static string ApplyFewShot(string basePrompt, List<KeyValuePair<string, string>> examples)
        {
            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine("以下是一些示例：\n");
            
            foreach (var example in examples)
            {
                prompt.AppendLine($"输入: {example.Key}");
                prompt.AppendLine($"输出: {example.Value}");
                prompt.AppendLine();
            }
            
            prompt.AppendLine("现在，请按照上面的示例格式回答下面的问题：\n");
            prompt.AppendLine(basePrompt);
            
            return prompt.ToString();
        }
        
        /// <summary>
        /// 应用思考-行动-观察（ReAct）提示策略
        /// </summary>
        public static string ApplyReAct(string basePrompt)
        {
            return $@"请使用ReAct方法（思考-行动-观察-思考）来解决以下问题：
                    {basePrompt}
                    请按照以下格式回答：
                    思考：[你对问题的初步思考]
                    行动：[你决定采取的行动]
                    观察：[从行动中得到的观察结果]
                    思考：[基于观察结果的进一步思考]
                    行动：[下一步行动]
                    ...
                    结论：[最终结论]";
        }
        
        /// <summary>
        /// 应用思维树（Tree of Thoughts）提示策略
        /// </summary>
        public static string ApplyTreeOfThoughts(string basePrompt, int branches = 3)
        {
            return $@"请使用思维树方法来解决以下问题：
                    {basePrompt}
                    请考虑{branches}种不同的思考路径，每个路径探索一个不同的可能性：
                    思路1：[第一种解决思路]
                    - 步骤1: ...
                    - 步骤2: ...
                    - 结果: ...
                    思路2：[第二种解决思路]
                    - 步骤1: ...
                    - 步骤2: ...
                    - 结果: ...
                    思路3：[第三种解决思路]
                    - 步骤1: ...
                    - 步骤2: ...
                    - 结果: ...
                    最终结论：[综合以上思路得出的最佳答案]";
        }
        
        /// <summary>
        /// 创建自洽性检查提示
        /// </summary>
        public static string ApplySelfConsistency(string basePrompt)
        {
            return $@"请解决以下问题，并确保你的回答是自洽的：
                    {basePrompt}
                    请按照以下步骤回答：
                    1. 初步分析问题
                    2. 提出几种可能的解决方案
                    3. 分析每种解决方案的优缺点
                    4. 检查你的推理过程是否存在逻辑矛盾
                    5. 如果发现矛盾，请修正你的推理
                    6. 给出最终答案";
        }
        
        /// <summary>
        /// 创建检索增强生成（RAG）风格的提示词
        /// </summary>
        public static string ApplyRAG(string basePrompt, List<string> retrievedContexts)
        {
            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine("以下是与问题相关的背景信息：\n");
            
            for (int i = 0; i < retrievedContexts.Count; i++)
            {
                prompt.AppendLine($"[参考资料 {i+1}]");
                prompt.AppendLine(retrievedContexts[i]);
                prompt.AppendLine();
            }
            
            prompt.AppendLine("请基于上述参考资料回答以下问题：\n");
            prompt.AppendLine(basePrompt);
            prompt.AppendLine("\n请确保你的回答完全基于提供的参考资料。如果参考资料中没有提到，请清楚说明。");
            
            return prompt.ToString();
        }
        
        /// <summary>
        /// 根据指定策略应用提示词工程技术
        /// </summary>
        public static string ApplyStrategy(string basePrompt, PromptStrategy strategy, object? additionalParams = null)
        {
            switch (strategy)
            {
                case PromptStrategy.CoT:
                    return ApplyCoT(basePrompt);
                    
                case PromptStrategy.FewShot:
                    if (additionalParams is List<KeyValuePair<string, string>> examples)
                        return ApplyFewShot(basePrompt, examples);
                    throw new ArgumentException("FewShot策略需要提供示例列表");
                    
                case PromptStrategy.ReAct:
                    return ApplyReAct(basePrompt);
                    
                case PromptStrategy.TreeOfThoughts:
                    int branches = 3;
                    if (additionalParams is int b)
                        branches = b;
                    return ApplyTreeOfThoughts(basePrompt, branches);
                    
                case PromptStrategy.SelfConsistency:
                    return ApplySelfConsistency(basePrompt);
                    
                case PromptStrategy.RAG:
                    if (additionalParams is List<string> contexts)
                        return ApplyRAG(basePrompt, contexts);
                    throw new ArgumentException("RAG策略需要提供检索上下文列表");
                    
                case PromptStrategy.ZeroShot:
                    return $"请回答以下问题，无需额外解释：\n\n{basePrompt}";
                    
                case PromptStrategy.Direct:
                default:
                    return basePrompt;
            }
        }
    }
} 