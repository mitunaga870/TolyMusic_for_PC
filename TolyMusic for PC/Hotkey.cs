using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace TolyMusic_for_PC;

public class Hotkey : IDisposable
{
    public event EventHandler HotkeyPressed;
    private HotKeyForm form;
    
    //ホットキーの登録
    public Hotkey( Keys key)
    {
        //ホットキーの登録
        form = new HotKeyForm(key,OnHotkeyPressed);
        
    }
    
    //ホットキーが押されたときの処理
    private void OnHotkeyPressed()
    {
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }
    
    //ホットキーの解除
    public void Dispose()
    {
        form.Dispose();
    }
    
    //ホットキーの登録用フォーム
    private class HotKeyForm : Form 
    {
        //ホットキーの登録関数呼び出し
        [DllImport("user32.dll")]
        extern static int RegisterHotKey(IntPtr HWnd, int ID, uint MOD_KEY, uint KEY);
        [DllImport("user32.dll")]
        extern static int UnregisterHotKey(IntPtr HWnd, int ID);
        
        const int WM_HOTKEY = 0x0312;
        private int id;
        private ThreadStart proc;
        
        public HotKeyForm(Keys key,ThreadStart proc)
        {
            this.proc = proc;
            
            for (int i = 0; i < 0x10000; i++)
            {
                if (RegisterHotKey(this.Handle, i, 0, (uint)key) != 0)
                {
                    id = i;
                    break;
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            
            if (m.Msg == WM_HOTKEY)
            {
                if ((int)m.WParam == id)
                {
                    proc();
                }
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            UnregisterHotKey(this.Handle, id);
            base.Dispose(disposing);
        }
    }
}