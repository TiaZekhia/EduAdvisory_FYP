import { useEffect, useState } from "react";
import { profileApi } from "../../../../services/profileApi";

export const useStudentProfile = () => {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        setLoading(true);
        setError("");
        const response = await profileApi.getMyProfile();
        setProfile(response?.data?.studentProfile);
      } catch (err) {
        console.error("Error fetching student profile:", err);
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
