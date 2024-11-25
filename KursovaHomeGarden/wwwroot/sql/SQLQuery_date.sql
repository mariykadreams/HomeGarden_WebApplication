CREATE PROCEDURE GetPlantActionsForPeriod
    @StartDate DATE,
    @EndDate DATE,
    @PlantId INT
AS
BEGIN
    SELECT 
        p.plant_id,
        p.plant_name,
        s.season_name,
        at.type_name,
        af.Interval,
        af.volume,
        af.notes,
        f.fert_name,
        f.fert_description
    FROM 
        ActionFrequencies af -- Changed from ActionFrequency to ActionFrequencies
    INNER JOIN 
        Plants p ON af.plant_id = p.plant_id
    INNER JOIN 
        Seasons s ON af.season_id = s.season_id
    INNER JOIN 
        ActionTypes at ON af.action_type_id = at.action_type_id
    LEFT JOIN 
        Fertilizes f ON af.Fert_type_id = f.Fert_type_id
    WHERE 
        p.plant_id = @PlantId
        AND (
            (@StartDate BETWEEN s.season_start AND s.season_end)
            OR (@EndDate BETWEEN s.season_start AND s.season_end)
            OR (s.season_start BETWEEN @StartDate AND @EndDate)
        )
    ORDER BY 
        s.season_start, at.type_name, af.Interval;
END;
