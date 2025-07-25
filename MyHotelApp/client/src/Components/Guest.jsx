import '../css/Guest.css';
export default function Guest() {
return (
  <form className="guest-form">
    <div className="form-group">
      <label className="form-label">Full Name:</label>
      <input type="text" name="fullName" className="form-input" />
    </div>

    <div className="form-group">
      <label className="form-label">JMBG:</label>
      <input type="text" name="jmbg" className="form-input" />
    </div>

    <div className="form-group">
      <label className="form-label">Phone Number:</label>
      <input type="text" name="phoneNumber" className="form-input" placeholder='+381'/>
    </div>

    <button type="submit" className="form-button">Submit</button>
  </form>
);


}