using System.IO;

namespace pdq
{
  class GPULineBreakCounter : CPULineBreakCounter
  {
    public GPULineBreakCounter(FileInfo fileInfo):base(fileInfo)
    {
    }
    public override bool GetCount(out int lineBreakCount)
    {
      // check if this is an N-Series virtual machine on the Azure cloud
      // if not use base.GetCount

      // base.GetCount could be called anyway for small files maybe?
      // benchmark tests would reveal it if this is needed or not
      // question to be answered outside of the scope of an interview test
      return base.GetCount(out lineBreakCount);

      // note: the interview test instructions
      // stated "You may use C#, VB, or Java"
      // if cuda were an option the memory could be
      // copied from the host to the GPU device 
      // and immediately freed 

      // GPU's can do millions if not billions of parallel operations 
      // CPU can do only number of cores parallel operations without
      // swapping/sharing

      // the GPU would probably return results much faster
      // shared memory may possibly be used to tally up results of
      // counts of portions of the file

      // no need to tie up host memory while the count occurs
    }
  }
}
