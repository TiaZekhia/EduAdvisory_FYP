using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models
{
    [Table("course_prerequisite")]
    public class CoursePrerequisite
    {
        [Column("course_code")]
        public string CourseCode { get; set; }

        [Column("prerequisite_course_code")]
        public string PrerequisiteCourseCode { get; set; }

        public SisCourse Course { get; set; }
        public SisCourse PrerequisiteCourse { get; set; }
    }

}
