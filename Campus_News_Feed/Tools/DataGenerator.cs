using System;
using System.Collections.Generic;
using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campus_News_Feed.Tools
{
    public static class DataGenerator
    {
        private static readonly Random Random = new Random();
        
        public static async Task GenerateNewsData(AppDbContext dbContext, int count = 50)
        {
            // 获取现有分类
            var categories = await dbContext.Categories.ToListAsync();
            if (!categories.Any())
            {
                throw new InvalidOperationException("没有可用的新闻分类，请先创建分类");
            }

            var newsList = new List<News>();
            
            // 定义各类型的新闻标题
            var titlesByType = new Dictionary<string, string[]>
            {
                // 校园动态新闻标题
                { "校园动态", new string[]
                    {
                        "学校召开2024年度教学工作会议",
                        "我校获评全国高校数字化转型示范单位",
                        "校长在开学典礼上发表重要讲话",
                        "我校成功举办第十届校园文化节",
                        "学校与多家企业签订战略合作协议",
                        "2023年度优秀教师表彰大会隆重举行",
                        "校友捐资设立百万奖学金",
                        "新学期食堂推出多款新菜品",
                        "校园安全防范月活动圆满结束",
                        "学校图书馆新增万册藏书",
                        "校园网络升级改造工程完成",
                        "校学生会完成换届选举工作",
                        "校园开放日活动吸引千余名高中生参观"
                    }
                },
                
                // 学术资讯新闻标题
                { "学术资讯", new string[]
                    {
                        "我校教授在Nature发表重要研究成果",
                        "国际人工智能学术研讨会在我校召开",
                        "我校科研团队获国家自然科学基金重点项目",
                        "计算机学院举办区块链技术讲座",
                        "诺贝尔奖得主来校进行学术交流",
                        "第五届研究生学术论坛圆满结束",
                        "我校三项成果获省科学技术奖",
                        "教授团队研发新型环保材料",
                        "学校承办全国青年教师教学技能大赛",
                        "医学院与三甲医院共建实验室",
                        "跨学科研究中心正式揭牌",
                        "数学建模大赛校内选拔赛开始"
                    }
                },
                
                // 体育赛事新闻标题
                { "体育赛事", new string[]
                    {
                        "我校在全国大学生运动会上获5金3银",
                        "校园足球联赛圆满落幕",
                        "教职工趣味运动会成功举办",
                        "我校举办第30届校园运动会",
                        "校篮球队获省高校联赛冠军",
                        "阳光长跑活动全面启动",
                        "国际象棋比赛吸引众多爱好者参与",
                        "游泳馆改造工程竣工并投入使用",
                        "大学生马拉松比赛激情开跑",
                        "校田径队选手打破省记录"
                    }
                },
                
                // 文化活动新闻标题
                { "文化活动", new string[]
                    {
                        "校园歌手大赛完美收官",
                        "大学生艺术团赴海外演出",
                        "第十二届读书节启动仪式举行",
                        "外国留学生中国文化体验活动",
                        "校园摄影大赛获奖作品展",
                        "经典诵读比赛展现传统文化魅力",
                        "国际文化交流周活动丰富多彩",
                        "戏剧社新剧《青春》首演大获成功",
                        "民族文化展示周活动启动",
                        "校园原创音乐大赛开始报名"
                    }
                },
                
                // 就业指导新闻标题
                { "就业指导", new string[]
                    {
                        "2024年春季校园招聘会成功举办",
                        "知名企业HR分享求职经验",
                        "简历制作与面试技巧讲座",
                        "毕业生就业率创历史新高",
                        "创业指导中心提供一对一咨询服务",
                        "校企合作推出定向培养计划",
                        "海外就业项目说明会",
                        "实习基地建设取得新进展",
                        "公务员考试辅导班开课",
                        "职业生涯规划大赛启动",
                        "校友创业分享会在学术报告厅举行",
                        "就业资讯速递：多家企业发布招聘需求"
                    }
                },
                
                // 默认标题（当分类名称不匹配时使用）
                { "默认", new string[]
                    {
                        "最新消息：重要通知发布",
                        "校内新闻：近期活动预告",
                        "通知公告：关于近期安排的说明",
                        "重要信息：请所有学生注意",
                        "校内活动：参与方式与流程",
                        "系统通知：新功能上线",
                        "特别报道：校内热点话题",
                        "每周速递：校园近况汇总",
                        "校园快讯：本周要闻",
                        "关注：校园生活新动向"
                    }
                }
            };
            
            // 新闻内容模板
            var contentTemplates = new string[]
            {
                "近日，{0}。活动由{1}主办，吸引了众多{2}参与。据了解，本次活动旨在{3}，对于{4}具有重要意义。{5}表示，将继续推动{6}的发展。",
                
                "日前，{0}。本次{1}由学校{2}负责组织，得到了{3}的大力支持。参与者纷纷表示，通过此次{4}，收获了丰富的{5}，希望今后能有更多类似的{6}。",
                
                "{0}于近期举行。据{1}介绍，此次{2}是为了响应{3}的号召，为{4}提供更好的服务。活动现场，{5}进行了精彩的发言。下一步，学校将继续加强{6}，为创建一流大学贡献力量。",
                
                "本周，{0}。此次{1}是我校{2}的重要组成部分，对提高{3}水平具有积极作用。{4}对本次活动给予了高度评价，认为对促进{5}起到了推动作用。未来，学校将进一步加强{6}建设。",
                
                "学校于{0}举办了{1}。作为{2}的关键环节，此次活动得到了{3}的广泛关注。多位{4}参与了此次活动并表示，这对{5}具有深远影响。后续，学校将继续完善{6}机制，提供更优质的服务。"
            };
            
            // 活动类型
            var eventTypes = new string[] { "论坛", "讲座", "比赛", "展览", "交流会", "研讨会", "培训", "庆典", "演出", "表彰会" };
            
            // 组织者
            var organizers = new string[] { "学生处", "教务处", "校团委", "研究生院", "国际交流处", "各学院", "校学生会", "校工会", "校友会", "社团联合会" };
            
            // 参与者
            var participants = new string[] { "学生", "教师", "校友", "企业代表", "专家学者", "社会各界人士", "留学生", "高中生", "研究生", "青年教师" };
            
            // 目的
            var purposes = new string[] { 
                "提高学生综合素质", "促进学术交流", "加强校企合作", "培养创新人才", "弘扬传统文化", 
                "增强校园文化氛围", "提升就业竞争力", "展示学校教学成果", "加强国际交流", "服务地方经济发展" 
            };
            
            // 意义
            var significances = new string[] { 
                "学生成长", "学校发展", "教育改革", "学术研究", "校园文化建设", 
                "人才培养", "国际化进程", "教学质量提升", "学风建设", "创新创业教育" 
            };
            
            // 发言人
            var speakers = new string[] { "校领导", "学院院长", "专家", "企业负责人", "学生代表", "教师代表", "校友代表", "嘉宾", "与会专家", "主办方负责人" };
            
            // 发展领域
            var developmentAreas = new string[] { 
                "教学改革", "科研创新", "人才培养", "国际交流", "校园文化", 
                "学生工作", "信息化建设", "创新创业教育", "产学研合作", "学科建设" 
            };
            
            // 日期模板
            var dateTemplates = new string[] {
                "昨日", "近日", "本周三", "上周末", "本月初", 
                "上午", "今天下午", "周五晚", "本学期开学初", "本月15日"
            };

            // 生成新闻
            for (int i = 0; i < count; i++)
            {
                // 随机选择一个分类
                var category = categories[Random.Next(categories.Count)];
                
                // 匹配分类名称获取对应的标题数组，如果找不到则使用默认标题
                string[] titles;
                if (!titlesByType.TryGetValue(category.Name, out titles))
                {
                    // 如果没有找到匹配的分类标题，则使用默认标题
                    titles = titlesByType["默认"];
                }
                
                // 从对应分类的标题中随机选择一个
                var title = titles[Random.Next(titles.Length)];
                
                // 随机选择一个内容模板
                var contentTemplate = contentTemplates[Random.Next(contentTemplates.Length)];
                
                // 填充内容模板
                var content = string.Format(contentTemplate,
                    title,
                    eventTypes[Random.Next(eventTypes.Length)],
                    organizers[Random.Next(organizers.Length)],
                    participants[Random.Next(participants.Length)],
                    purposes[Random.Next(purposes.Length)],
                    speakers[Random.Next(speakers.Length)],
                    developmentAreas[Random.Next(developmentAreas.Length)]
                );
                
                // 创建新闻实体
                var news = new News
                {
                    Title = title,
                    Content = content,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 60)), // 随机生成1-60天前的日期
                    UpdatedAt = null,
                    ClickCount = Random.Next(0, 1000) // 随机点击量
                };
                
                newsList.Add(news);
            }
            
            // 保存到数据库
            await dbContext.News.AddRangeAsync(newsList);
            await dbContext.SaveChangesAsync();
        }
    }
} 