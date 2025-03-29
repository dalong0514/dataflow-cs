using dataflow_cs.Domain.ValueObjects;
using dataflow_cs.Domain.Repositories.Interfaces;

namespace dataflow_cs.Business.Services
{
    /// <summary>
    /// 菜单配置应用服务
    /// </summary>
    public class MenuConfigService
    {
        private readonly IMenuConfigRepository _repository;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">菜单配置仓储</param>
        public MenuConfigService(IMenuConfigRepository repository)
        {
            _repository = repository;
        }
        
        /// <summary>
        /// 加载菜单配置
        /// </summary>
        /// <returns>菜单配置</returns>
        public MenuConfig LoadMenuConfig()
        {
            return _repository.Load();
        }
        
        /// <summary>
        /// 保存菜单配置
        /// </summary>
        /// <param name="config">要保存的菜单配置</param>
        public void SaveMenuConfig(MenuConfig config)
        {
            _repository.Save(config);
        }
        
        /// <summary>
        /// 添加菜单组
        /// </summary>
        /// <param name="title">组标题</param>
        /// <param name="iconKey">图标键</param>
        /// <returns>添加的菜单组</returns>
        public MenuGroup AddMenuGroup(string title, string iconKey = "folder")
        {
            MenuConfig config = LoadMenuConfig();
            
            MenuGroup group = new MenuGroup
            {
                Title = title,
                IconKey = iconKey
            };
            
            config.MenuGroups.Add(group);
            SaveMenuConfig(config);
            
            return group;
        }
        
        /// <summary>
        /// 添加菜单项
        /// </summary>
        /// <param name="groupTitle">组标题</param>
        /// <param name="title">菜单项标题</param>
        /// <param name="command">命令</param>
        /// <param name="iconKey">图标键</param>
        /// <returns>是否添加成功</returns>
        public bool AddMenuItem(string groupTitle, string title, string command, string iconKey = "command")
        {
            MenuConfig config = LoadMenuConfig();
            
            // 查找菜单组
            MenuGroup group = config.MenuGroups.Find(g => g.Title == groupTitle);
            if (group == null)
            {
                return false;
            }
            
            // 添加菜单项
            group.Items.Add(new MenuItem
            {
                Title = title,
                Command = command,
                IconKey = iconKey
            });
            
            SaveMenuConfig(config);
            return true;
        }
    }
} 