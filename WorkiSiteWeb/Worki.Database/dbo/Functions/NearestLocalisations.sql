
CREATE FUNCTION [dbo].[NearestLocalisations](@lat [real], @long [real], @distance [real])
RETURNS TABLE AS 
RETURN
SELECT Localisation.ID
FROM Localisation
WHERE dbo.DistanceBetween(@lat, @long, Latitude, Longitude) < @distance

