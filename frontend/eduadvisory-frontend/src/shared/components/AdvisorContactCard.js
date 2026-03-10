import "./AdvisorContactCard.css";
export default function AdvisorContactCard({
  title,
  subtitle,
  intro,
  advisor,
  showOffice = true,
  showAvailability = false,
  availabilityNode = null,
  footerNote,
}) {
  return (
    <div>
      <div className="fw-semibold fs-4">{title}</div>
      {subtitle ? <div className="text-muted mb-3">{subtitle}</div> : null}

      {intro ? <div className="advisor-help-text mb-3">{intro}</div> : null}

      <div className="advisor-contact-box">
        <div className="d-flex align-items-center justify-content-between flex-wrap gap-3 mb-3">
          <div>
            <div className="advisor-contact-label">Your Academic Advisor</div>
            <div className="advisor-contact-name">
              {advisor?.name ?? "Not assigned"}
            </div>
          </div>

          {showAvailability ? availabilityNode : null}
        </div>

        <div className="row g-3">
          <div className="col-12 col-md-6">
            <div className="advisor-contact-tile">
              <div className="advisor-contact-tile-label">
                <i className="pi pi-envelope" />
                Email
              </div>
              <div className="advisor-contact-tile-value">
                {advisor?.email ?? "-"}
              </div>
            </div>
          </div>

          {showOffice && (
            <div className="col-12 col-md-6">
              <div className="advisor-contact-tile">
                <div className="advisor-contact-tile-label">
                  <i className="pi pi-building" />
                  Office
                </div>
                <div className="advisor-contact-tile-value">
                  {advisor?.office ?? "Building A, Room 305"}
                </div>
              </div>
            </div>
          )}

          <div className="col-12 col-md-6">
            <div className="advisor-contact-tile">
              <div className="advisor-contact-tile-label">
                <i className="pi pi-clock" />
                Office Hours
              </div>
              <div className="advisor-contact-tile-value">
                {advisor?.officeHours ?? "Mon, Wed, Fri: 2-4 PM"}
              </div>
            </div>
          </div>
        </div>

        {footerNote ? (
          <div className="advisor-contact-note mt-3">{footerNote}</div>
        ) : null}
      </div>
    </div>
  );
}