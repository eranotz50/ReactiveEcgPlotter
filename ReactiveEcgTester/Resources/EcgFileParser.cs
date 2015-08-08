using System;
using System.Collections.Generic;
using System.IO;
using Domain;


namespace ReactiveEcgTester.Resources
{
	public interface IEcgParser
	{
		List<EcgSample> Parse();
	}

	public class EcgFileParser : IEcgParser
    {
        private string _ecgFilePath;

        public EcgFileParser(string ecgFilePath)
        {
            _ecgFilePath = ecgFilePath;
        }
        public List<EcgSample> Parse()
        {
			var ecgSamples = new List<EcgSample>();
            using (var reader = new StreamReader(File.Open(_ecgFilePath, FileMode.Open)))
            {
                while (!reader.EndOfStream)
                {
                    var ecgLine = reader.ReadLine().Split(',');
                    var ecgValue = Convert.ToInt16(Convert.ToDouble(ecgLine[1]));
                    var isRwave = Convert.ToBoolean(Convert.ToInt16(Convert.ToDouble(ecgLine[0])));
					ecgSamples.Add(new EcgSample(ecgValue, isRwave ? ECGFlags.RWave : ECGFlags.None));
                }
            }
            return ecgSamples;
        }
    }
}
