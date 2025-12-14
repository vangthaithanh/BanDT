window.onload = function () {
    const ten = localStorage.getItem("tenDangNhap");
    const userActions = document.getElementById("userActions");

    if (ten) {
        userActions.innerHTML = `
            <div class="dropdown">
                <a class="nav-link dropdown-toggle" href="#" id="userDropdown" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="fas fa-user"></i> Xin chào, <strong>${ten}</strong>
                </a>
                <ul class="dropdown-menu dropdown-menu-end mt-2 shadow rounded" aria-labelledby="userDropdown">
                    <li>
                        <a class="dropdown-item d-flex align-items-center text-danger" href="#" id="btnDangXuat">
                            <i class="fas fa-sign-out-alt me-2"></i> Đăng xuất
                        </a>
                    </li>
                </ul>
            </div>
        `;


        document.getElementById("btnDangXuat").addEventListener("click", function (e) {
            e.preventDefault();
            localStorage.removeItem("tenDangNhap");
            window.location.href = "2_TrangChu.html";
        });
    }
};