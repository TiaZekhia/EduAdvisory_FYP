import { useEffect, useState } from "react";
import { profileApi } from "../../../../services/profileApi";

export const useAdvisorProfile = () => {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        setLoading(true);
        setError("");
        const response = await profileApi.getMyProfile();
        setProfile(response?.data?.advisorProfile);
      } catch (err) {
        console.error("Error fetching advisor profile:", err);
        setError(
          err?.response?.data?.message ||
            err?.message ||
            "Failed to load profile"
        );
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, []);

  return { profile, loading, error };
};
