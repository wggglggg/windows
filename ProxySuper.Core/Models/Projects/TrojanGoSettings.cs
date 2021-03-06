﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class TrojanGoSettings : IProjectSettings
    {
        public override List<int> FreePorts
        {
            get
            {
                return new List<int>();
            }
        }

        public override ProjectType Type { get; set; } = ProjectType.TrojanGo;

        /// <summary>
        /// 域名
        /// </summary>
        public override string Domain { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public override int Port { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 伪装域名
        /// </summary>
        public string MaskDomain { get; set; }
    }
}
