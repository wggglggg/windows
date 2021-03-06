﻿using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySU_Core.Models.Developers
{
    public class TrojanGoProject : Project<TrojanGoSettings>
    {
        public TrojanGoProject(SshClient sshClient, TrojanGoSettings parameters, Action<string> writeOutput) : base(sshClient, parameters, writeOutput)
        {
        }

        public override void Install()
        {
            try
            {
                EnsureRootAuth();

                if (FileExists("/usr/local/bin/xray"))
                {
                    var btnResult = MessageBox.Show("已经安装Xray，是否需要重装？", "提示", MessageBoxButton.YesNo);
                    if (btnResult == MessageBoxResult.No)
                    {
                        MessageBox.Show("安装终止", "提示");
                        return;
                    }
                }

                WriteOutput("检测安装系统环境...");
                EnsureSystemEnv();
                WriteOutput("检测安装系统环境完成");

                WriteOutput("配置服务器端口...");
                ConfigurePort();
                WriteOutput("端口配置完成");

                WriteOutput("安装必要的系统工具...");
                ConfigureSoftware();
                WriteOutput("系统工具安装完成");

                WriteOutput("检测IP6...");
                ConfigureIPv6();
                WriteOutput("检测IP6完成");

                WriteOutput("配置防火墙...");
                ConfigureFirewall();
                WriteOutput("防火墙配置完成");

                WriteOutput("同步系统和本地时间...");
                SyncTimeDiff();
                WriteOutput("时间同步完成");

                WriteOutput("检测域名是否绑定本机IP...");
                ValidateDomain();
                WriteOutput("域名检测完成");

                WriteOutput("安装Trojan-Go...");
                InstallTrojanGo();
                WriteOutput("Trojan-Go安装完成");

                WriteOutput("安装Caddy...");
                InstallCaddy();
                WriteOutput("Caddy安装完成");

                WriteOutput("启动BBR");
                EnableBBR();

                UploadCaddyFile();
                WriteOutput("************");
                WriteOutput("安装完成，尽情享用吧......");
                WriteOutput("************");
            }
            catch (Exception ex)
            {
                var errorLog = "安装终止，" + ex.Message;
                WriteOutput(errorLog);
                MessageBox.Show(errorLog);
            }
        }

        private void InstallTrojanGo()
        {
            WriteOutput("安装Trojan-Go");
            RunCmd(@"curl https://raw.githubusercontent.com/proxysu/shellscript/master/trojan-go.sh yes | bash");
            var success = FileExists("/usr/local/etc/trojan-go");
            if (success == false)
            {
                throw new Exception("trojan-go 安装失败，请联系开发者！");
            }

            RunCmd($"sed -i 's/User=nobody/User=root/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/CapabilityBoundingSet=/#CapabilityBoundingSet=/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/AmbientCapabilities=/#AmbientCapabilities=/g' /etc/systemd/system/xray.service");
            RunCmd($"systemctl daemon-reload");

            RunCmd("systemctl enable trojan-go");
            RunCmd("systemctl start trojan-go");
            WriteOutput("Trojan-Go 安装完成");

            InstallCert(
                dirPath: "/usr/local/etc/trojan-go",
                certName: "trojan-go.crt",
                keyName: "trojan-go.key");

            if (FileExists("/usr/local/etc/trojan-go/config.json"))
            {
                RunCmd("mv /usr/local/etc/trojan-go/config.json config.json.old");
            }

            // 上传配置
            var settings = TrojanGoConfigBuilder.BuildTrojanGoConfig(Parameters);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(settings));
            UploadFile(stream, "/usr/local/etc/trojan-go/config.json");
            RunCmd("systemctl restart trojan-go");
        }

        private void UploadCaddyFile(bool useCustomWeb = false)
        {
            var config = TrojanGoConfigBuilder.BuildCaddyConfig(Parameters, useCustomWeb);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(config));
            if (FileExists("/etc/caddy/Caddyfile"))
            {
                RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            }
            UploadFile(stream, "/etc/caddy/Caddyfile");
            RunCmd("systemctl restart caddy");
        }
    }

}
