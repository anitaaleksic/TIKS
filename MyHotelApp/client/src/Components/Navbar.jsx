import '../css/Navbar.css';
import { NavLink } from 'react-router-dom';
export default function Navbar() {
    return (
        <div className="navbar-wrapper">
            <nav className="navbar">
                <div className="navbar-content">
                    <ul className = "navbar-menu">
                        <li className="navbar-item">
                            <NavLink to="/guest" className={({ isActive }) => isActive ? "navbar-link-active" : "navbar-link"}>
                              Guest
                            </NavLink>
        
                        </li>

                        <li className="navbar-item">
                            <NavLink to="/reservation" className={({ isActive }) => isActive ? "navbar-link-active" : "navbar-link"}>
                              Reservation
                            </NavLink>
        
                        </li>

                        <li className="navbar-item">
                            <NavLink to="/room" className={({ isActive }) => isActive ? "navbar-link-active" : "navbar-link"}>
                              Room
                            </NavLink>
        
                        </li>

                        <li className="navbar-item">
                            <NavLink to="/roomservice" className={({ isActive }) => isActive ? "navbar-link-active" : "navbar-link"}>
                              Room Service
                            </NavLink>
        
                        </li>

                        <li className="navbar-item">
                            <NavLink to="/extraservice" className={({ isActive }) => isActive ? "navbar-link-active" : "navbar-link"}>
                              Extra Service
                            </NavLink>
        
                        </li>
                    </ul>
                </div>
            </nav>
        </div>
    )
}