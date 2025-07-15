using Syncfusion.PMML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Tests
{

    /// <summary>
    //| Purpose               | How it works                                                     |
    //| --------------------  | ---------------------------------------------------------------- |
    //| ** Before each test** | The** constructor** of the test class is called.                 |
    //| ** After each test**  | The `Dispose()` method is called if you implement `IDisposable`. |

    /// </summary>
    public class MyFixture : IDisposable
    {
        public MyFixture()
        {
            // Like [ClassInitialize]
            Debug.WriteLine("ClassInitialize");
        }

        public void Dispose()
        {
            // Like [ClassCleanup]
            Debug.WriteLine("ClassCleanup");
        }
    }

}
