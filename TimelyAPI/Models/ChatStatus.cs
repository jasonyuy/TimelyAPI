namespace TimelyAPI.Models
{
    /// <summary>
    /// enum to show conversation status.
    /// </summary>
    enum ChatStatus
    {
        /// <summary>
        /// User has not started the conversation.
        /// </summary>
        None,
        /// <summary>
        /// The bridge of a knock-knock joke.
        /// </summary>
        Bridge,
        /// <summary>
        /// Punchline of a knock-knock joke.
        /// </summary>
        Punchline,
        /// <summary>
        /// Unknown equipment, should ask user for equipment. 
        /// </summary>
        UnkEquipment,
        /// <summary>
        /// Unknown station, should ask user for station.
        /// </summary>
        UnkStation,
        /// <summary>
        /// Should ask user to specify offline, online, or media pH.
        /// </summary>
        Specify_pH,
        /// <summary>
        /// Should ask user to specify offline or online dO2.
        /// </summary>
        Specify_dO2,
        /// <summary>
        /// Should ask user to specify which titer result.
        /// </summary>
        SpecifyTiter,
        /// <summary>
        /// Missing batch identifier. 
        /// </summary>
        UnkBatch,
        /// <summary>
        /// Should ask user to specify target paramter.
        /// </summary>
        SpecifyTarget,
        /// <summary>
        /// Generic enum that means user should specify something.
        /// </summary>
        Specify,
    };
}