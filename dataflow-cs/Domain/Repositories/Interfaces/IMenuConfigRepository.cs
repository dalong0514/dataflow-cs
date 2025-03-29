using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dataflow_cs.Domain.ValueObjects;

namespace dataflow_cs.Domain.Repositories.Interfaces
{
    /// <summary>
    /// 菜单配置仓储接口
    /// </summary>
    public interface IMenuConfigRepository
    {
        /// <summary>
        /// 加载菜单配置
        /// </summary>
        /// <returns>菜单配置对象</returns>
        MenuConfig Load();

        /// <summary>
        /// 保存菜单配置
        /// </summary>
        /// <param name="config">要保存的菜单配置</param>
        void Save(MenuConfig config);
    }
}
