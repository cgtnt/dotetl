using ETLEngine.Interfaces;

namespace ETLEngineTests
{
    internal class PersonTDataExample
    {
        [PotentialInputKeys(["name"])]
        public string FullName { get; set; }
        public int Age { get; set; }
    }

    internal class MegaPersonTDataExample
    {
        public string FirstName { get; set; }
        public int Age { get; set; }
        public bool RadioAmateurLicensed { get; set; }
    }
}
